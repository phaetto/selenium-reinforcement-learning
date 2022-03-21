const mojo = {
  internal: { interfaceSupport: {} },
  interfaceControl: {},
  pipeControl: {},
};
// Copyright 2018 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

/** @const {number} */
mojo.internal.kArrayHeaderSize = 8;

/** @const {number} */
mojo.internal.kStructHeaderSize = 8;

/** @const {number} */
mojo.internal.kUnionHeaderSize = 8;

/** @const {number} */
mojo.internal.kUnionDataSize = 16;

/** @const {number} */
mojo.internal.kMessageV0HeaderSize = 24;

/** @const {number} */
mojo.internal.kMessageV1HeaderSize = 32;

/** @const {number} */
mojo.internal.kMessageV2HeaderSize = 48;

/** @const {number} */
mojo.internal.kMapDataSize = 24;

/** @const {number} */
mojo.internal.kEncodedInvalidHandleValue = 0xffffffff;

/** @const {number} */
mojo.internal.kMessageFlagExpectsResponse = 1 << 0;

/** @const {number} */
mojo.internal.kMessageFlagIsResponse = 1 << 1;

/** @const {number} */
mojo.internal.kInterfaceNamespaceBit = 0x80000000;

/** @const {boolean} */
mojo.internal.kHostLittleEndian = (function() {
  const wordBytes = new Uint8Array(new Uint16Array([1]).buffer);
  return !!wordBytes[0];
})();

/**
 * @param {*} x
 * @return {boolean}
 */
mojo.internal.isNullOrUndefined = function(x) {
  return x === null || x === undefined;
};

/**
 * @param {number} size
 * @param {number} alignment
 * @return {number}
 */
mojo.internal.align = function(size, alignment) {
  return size + (alignment - (size % alignment)) % alignment;
};

/**
 * @param {!DataView} dataView
 * @param {number} byteOffset
 * @param {number|bigint} value
 */
mojo.internal.setInt64 = function(dataView, byteOffset, value) {
  if (mojo.internal.kHostLittleEndian) {
    dataView.setUint32(
        byteOffset, Number(BigInt(value) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
    dataView.setInt32(
        byteOffset + 4,
        Number((BigInt(value) >> BigInt(32)) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
  } else {
    dataView.setInt32(
        byteOffset, Number((BigInt(value) >> BigInt(32)) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
    dataView.setUint32(
        byteOffset + 4, Number(BigInt(value) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
  }
};

/**
 * @param {!DataView} dataView
 * @param {number} byteOffset
 * @param {number|bigint} value
 */
mojo.internal.setUint64 = function(dataView, byteOffset, value) {
  if (mojo.internal.kHostLittleEndian) {
    dataView.setUint32(
        byteOffset, Number(BigInt(value) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
    dataView.setUint32(
        byteOffset + 4,
        Number((BigInt(value) >> BigInt(32)) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
  } else {
    dataView.setUint32(
        byteOffset, Number((BigInt(value) >> BigInt(32)) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
    dataView.setUint32(
        byteOffset + 4, Number(BigInt(value) & BigInt(0xffffffff)),
        mojo.internal.kHostLittleEndian);
  }
};

/**
 * @param {!DataView} dataView
 * @param {number} byteOffset
 * @return {bigint}
 */
mojo.internal.getInt64 = function(dataView, byteOffset) {
  let low, high;
  if (mojo.internal.kHostLittleEndian) {
    low = dataView.getUint32(byteOffset, mojo.internal.kHostLittleEndian);
    high = dataView.getInt32(byteOffset + 4, mojo.internal.kHostLittleEndian);
  } else {
    low = dataView.getUint32(byteOffset + 4, mojo.internal.kHostLittleEndian);
    high = dataView.getInt32(byteOffset, mojo.internal.kHostLittleEndian);
  }
  return (BigInt(high) << BigInt(32)) | BigInt(low);
};

/**
 * @param {!DataView} dataView
 * @param {number} byteOffset
 * @return {bigint}
 */
mojo.internal.getUint64 = function(dataView, byteOffset) {
  let low, high;
  if (mojo.internal.kHostLittleEndian) {
    low = dataView.getUint32(byteOffset, mojo.internal.kHostLittleEndian);
    high = dataView.getUint32(byteOffset + 4, mojo.internal.kHostLittleEndian);
  } else {
    low = dataView.getUint32(byteOffset + 4, mojo.internal.kHostLittleEndian);
    high = dataView.getUint32(byteOffset, mojo.internal.kHostLittleEndian);
  }
  return (BigInt(high) << BigInt(32)) | BigInt(low);
};

/**
 * @typedef {{
 *   size: number,
 *   numInterfaceIds: (number|undefined),
 * }}
 */
mojo.internal.MessageDimensions;

/**
 * This computes the total amount of buffer space required to hold a struct
 * value and all its fields, including indirect objects like arrays, structs,
 * and nullable unions.
 *
 * @param {!mojo.internal.StructSpec} structSpec
 * @param {!Object} value
 * @return {!mojo.internal.MessageDimensions}
 */
mojo.internal.computeStructDimensions = function(structSpec, value) {
  let size = structSpec.packedSize;
  let numInterfaceIds = 0;
  for (const field of structSpec.fields) {
    let fieldValue = value[field.name];
    if (mojo.internal.isNullOrUndefined(fieldValue)) {
      fieldValue = field.defaultValue;
    }
    if (fieldValue === null) {
      continue;
    }

    if (field.type.$.computeDimensions) {
      const fieldDimensions =
          field.type.$.computeDimensions(fieldValue, field.nullable);
      size += mojo.internal.align(fieldDimensions.size, 8);
      numInterfaceIds += fieldDimensions.numInterfaceIds;
    } else if (field.type.$.hasInterfaceId) {
      numInterfaceIds++;
    }
  }
  return {size, numInterfaceIds};
};

/**
 * @param {!mojo.internal.UnionSpec} unionSpec
 * @param {!Object} value
 * @return {!mojo.internal.MessageDimensions}
 */
mojo.internal.computeUnionDimensions = function(unionSpec, nullable, value) {
  // Unions are normally inlined since they're always a fixed width of 16
  // bytes, but nullable union-typed fields require indirection. Hence this
  // unique special case where a union field requires additional storage
  // beyond the struct's own packed field data only when it's nullable.
  let size = nullable ? mojo.internal.kUnionDataSize : 0;
  let numInterfaceIds = 0;

  const keys = Object.keys(value);
  if (keys.length !== 1) {
    throw new Error(
        `Value for ${unionSpec.name} must be an Object with a ` +
        'single property named one of: ' +
        Object.keys(unionSpec.fields).join(','));
  }

  const tag = keys[0];
  const field = unionSpec.fields[tag];
  const fieldValue = value[tag];
  if (!mojo.internal.isNullOrUndefined(fieldValue)) {
    // Nested unions are always encoded with indirection, which we induce by
    // claiming the field is nullable even if it's not.
    if (field['type'].$.computeDimensions) {
      const nullable = !!field['type'].$.unionSpec || field['nullable'];
      const fieldDimensions =
          field['type'].$.computeDimensions(fieldValue, nullable);
      size += mojo.internal.align(fieldDimensions.size, 8);
      numInterfaceIds += fieldDimensions.numInterfaceIds;
    } else if (field['type'].$.hasInterfaceId) {
      numInterfaceIds++;
    }
  }

  return {size, numInterfaceIds};
};

/**
 * @param {!mojo.internal.ArraySpec} arraySpec
 * @param {!Array|!Uint8Array} value
 * @return {number}
 */
mojo.internal.computeInlineArraySize = function(arraySpec, value) {
  if (arraySpec.elementType === mojo.internal.Bool) {
    return mojo.internal.kArrayHeaderSize + (value.length + 7) >> 3;
  } else {
    return mojo.internal.kArrayHeaderSize +
        value.length *
        arraySpec.elementType.$.arrayElementSize(!!arraySpec.elementNullable);
  }
};

/**
 * @param {!mojo.internal.ArraySpec} arraySpec
 * @param {!Array|!Uint8Array} value
 * @return {number}
 */
mojo.internal.computeTotalArraySize = function(arraySpec, value) {
  const inlineSize = mojo.internal.computeInlineArraySize(arraySpec, value);
  if (!arraySpec.elementType.$.computeDimensions)
    return inlineSize;

  let totalSize = inlineSize;
  for (let elementValue of value) {
    if (!mojo.internal.isNullOrUndefined(elementValue)) {
      totalSize += mojo.internal.align(
          arraySpec.elementType.$
              .computeDimensions(elementValue, !!arraySpec.elementNullable)
              .size,
          8);
    }
  }

  return totalSize;
};

/** Owns an outgoing message buffer and facilitates serialization. */
mojo.internal.Message = class {
  /**
   * @param {?mojo.internal.interfaceSupport.Endpoint} sender
   * @param {number} interfaceId
   * @param {number} flags
   * @param {number} ordinal
   * @param {number} requestId
   * @param {!mojo.internal.StructSpec} paramStructSpec
   * @param {!Object} value
   * @public
   */
  constructor(
      sender, interfaceId, flags, ordinal, requestId, paramStructSpec, value) {
    const dimensions =
        mojo.internal.computeStructDimensions(paramStructSpec, value);

    let headerSize, version;
    if (dimensions.numInterfaceIds > 0) {
      headerSize = mojo.internal.kMessageV2HeaderSize;
      version = 2;
    } else if (
        (flags &
         (mojo.internal.kMessageFlagExpectsResponse |
          mojo.internal.kMessageFlagIsResponse)) == 0) {
      headerSize = mojo.internal.kMessageV0HeaderSize;
      version = 0;
    } else {
      headerSize = mojo.internal.kMessageV1HeaderSize;
      version = 1;
    }

    const headerWithPayloadSize = headerSize + dimensions.size;
    const interfaceIdsSize = dimensions.numInterfaceIds > 0 ?
        mojo.internal.kArrayHeaderSize + dimensions.numInterfaceIds * 4 :
        0;
    const paddedInterfaceIdsSize = mojo.internal.align(interfaceIdsSize, 8);
    const totalMessageSize = headerWithPayloadSize + paddedInterfaceIdsSize;

    /** @public {!ArrayBuffer} */
    this.buffer = new ArrayBuffer(totalMessageSize);

    /** @public {!Array<MojoHandle>} */
    this.handles = [];

    const header = new DataView(this.buffer);
    header.setUint32(0, headerSize, mojo.internal.kHostLittleEndian);
    header.setUint32(4, version, mojo.internal.kHostLittleEndian);
    header.setUint32(8, interfaceId, mojo.internal.kHostLittleEndian);
    header.setUint32(12, ordinal, mojo.internal.kHostLittleEndian);
    header.setUint32(16, flags, mojo.internal.kHostLittleEndian);
    header.setUint32(20, 0);  // Padding
    if (version >= 1) {
      mojo.internal.setUint64(header, 24, requestId);
      if (version >= 2) {
        mojo.internal.setUint64(header, 32, BigInt(16));
        mojo.internal.setUint64(header, 40, BigInt(headerWithPayloadSize - 40));
        header.setUint32(
            headerWithPayloadSize, interfaceIdsSize,
            mojo.internal.kHostLittleEndian);
        header.setUint32(
            headerWithPayloadSize + 4, dimensions.numInterfaceIds || 0,
            mojo.internal.kHostLittleEndian);
      }
    }

    /** @private {number} */
    this.nextInterfaceIdIndex_ = 0;

    /** @private {?Uint32Array} */
    this.interfaceIds_ = null;

    if (dimensions.numInterfaceIds) {
      this.interfaceIds_ = new Uint32Array(
          this.buffer, headerWithPayloadSize + mojo.internal.kArrayHeaderSize,
          dimensions.numInterfaceIds);
    }

    /** @private {number} */
    this.nextAllocationOffset_ = headerSize;

    const paramStructData = this.allocate(paramStructSpec.packedSize);
    const encoder =
        new mojo.internal.Encoder(this, paramStructData, {endpoint: sender});
    encoder.encodeStructInline(paramStructSpec, value);
  }

  /**
   * @param {number} numBytes
   * @return {!DataView} A view into the allocated message bytes.
   */
  allocate(numBytes) {
    const alignedSize = mojo.internal.align(numBytes, 8);
    const view =
        new DataView(this.buffer, this.nextAllocationOffset_, alignedSize);
    this.nextAllocationOffset_ += alignedSize;
    return view;
  }
};

/**
 * Additional context to aid in encoding and decoding of message data.
 *
 * @typedef {{
 *   endpoint: ?mojo.internal.interfaceSupport.Endpoint,
 * }}
 */
mojo.internal.MessageContext;

/**
 * Helps encode outgoing messages. Encoders may be created recursively to encode
 * parial message fragments indexed by indirect message offsets, as with encoded
 * arrays and nested structs.
 */
mojo.internal.Encoder = class {
  /**
   * @param {!mojo.internal.Message} message
   * @param {!DataView} data
   * @param {?mojo.internal.MessageContext=} context
   * @public
   */
  constructor(message, data, context = null) {
    /** @const {?mojo.internal.MessageContext} */
    this.context_ = context;

    /** @private {!mojo.internal.Message} */
    this.message_ = message;

    /** @private {!DataView} */
    this.data_ = data;
  }

  encodeBool(byteOffset, bitOffset, value) {
    const oldValue = this.data_.getUint8(byteOffset);
    if (value)
      this.data_.setUint8(byteOffset, oldValue | (1 << bitOffset));
    else
      this.data_.setUint8(byteOffset, oldValue & ~(1 << bitOffset));
  }

  encodeInt8(offset, value) {
    this.data_.setInt8(offset, value);
  }

  encodeUint8(offset, value) {
    this.data_.setUint8(offset, value);
  }

  encodeInt16(offset, value) {
    this.data_.setInt16(offset, value, mojo.internal.kHostLittleEndian);
  }

  encodeUint16(offset, value) {
    this.data_.setUint16(offset, value, mojo.internal.kHostLittleEndian);
  }

  encodeInt32(offset, value) {
    this.data_.setInt32(offset, value, mojo.internal.kHostLittleEndian);
  }

  encodeUint32(offset, value) {
    this.data_.setUint32(offset, value, mojo.internal.kHostLittleEndian);
  }

  encodeInt64(offset, value) {
    mojo.internal.setInt64(this.data_, offset, value);
  }

  encodeUint64(offset, value) {
    mojo.internal.setUint64(this.data_, offset, value);
  }

  encodeFloat(offset, value) {
    this.data_.setFloat32(offset, value, mojo.internal.kHostLittleEndian);
  }

  encodeDouble(offset, value) {
    this.data_.setFloat64(offset, value, mojo.internal.kHostLittleEndian);
  }

  encodeHandle(offset, value) {
    this.encodeUint32(offset, this.message_.handles.length);
    this.message_.handles.push(value);
  }

  encodeAssociatedEndpoint(offset, endpoint) {
    console.assert(
        endpoint.isPendingAssociation, 'expected unbound associated endpoint');
    const sender = this.context_.endpoint;
    const id = sender.associatePeerOfOutgoingEndpoint(endpoint);
    const index = this.message_.nextInterfaceIdIndex_++;
    this.encodeUint32(offset, index);
    this.message_.interfaceIds_[index] = id;
  }

  encodeString(offset, value) {
    if (typeof value !== 'string')
      throw new Error('Unxpected non-string value for string field.');
    this.encodeArray(
        {elementType: mojo.internal.Uint8}, offset,
        mojo.internal.Encoder.stringToUtf8Bytes(value));
  }

  encodeOffset(offset, absoluteOffset) {
    this.encodeUint64(offset, absoluteOffset - this.data_.byteOffset - offset);
  }

  /**
   * @param {!mojo.internal.ArraySpec} arraySpec
   * @param {number} offset
   * @param {!Array|!Uint8Array} value
   */
  encodeArray(arraySpec, offset, value) {
    const arraySize = mojo.internal.computeInlineArraySize(arraySpec, value);
    const arrayData = this.message_.allocate(arraySize);
    const arrayEncoder =
        new mojo.internal.Encoder(this.message_, arrayData, this.context_);
    this.encodeOffset(offset, arrayData.byteOffset);

    arrayEncoder.encodeUint32(0, arraySize);
    arrayEncoder.encodeUint32(4, value.length);

    let byteOffset = 8;
    if (arraySpec.elementType === mojo.internal.Bool) {
      let bitOffset = 0;
      for (const e of value) {
        arrayEncoder.encodeBool(byteOffset, bitOffset, e);
        bitOffset++;
        if (bitOffset == 8) {
          bitOffset = 0;
          byteOffset++;
        }
      }
    } else {
      for (const e of value) {
        if (e === null) {
          if (!arraySpec.elementNullable) {
            throw new Error(
                'Trying to send a null element in an array of ' +
                'non-nullable elements');
          }
          arraySpec.elementType.$.encodeNull(arrayEncoder, byteOffset);
        }
        arraySpec.elementType.$.encode(
            e, arrayEncoder, byteOffset, 0, !!arraySpec.elementNullable);
        byteOffset += arraySpec.elementType.$.arrayElementSize(
            !!arraySpec.elementNullable);
      }
    }
  }

  /**
   * @param {!mojo.internal.MapSpec} mapSpec
   * @param {number} offset
   * @param {!Map|!Object} value
   */
  encodeMap(mapSpec, offset, value) {
    let keys, values;
    if (value.constructor.name == 'Map') {
      keys = Array.from(value.keys());
      values = Array.from(value.values());
    } else {
      keys = Object.keys(value);
      values = keys.map(k => value[k]);
    }

    const mapData = this.message_.allocate(mojo.internal.kMapDataSize);
    const mapEncoder =
        new mojo.internal.Encoder(this.message_, mapData, this.context_);
    this.encodeOffset(offset, mapData.byteOffset);

    mapEncoder.encodeUint32(0, mojo.internal.kMapDataSize);
    mapEncoder.encodeUint32(4, 0);
    mapEncoder.encodeArray({elementType: mapSpec.keyType}, 8, keys);
    mapEncoder.encodeArray(
        {
          elementType: mapSpec.valueType,
          elementNullable: mapSpec.valueNullable
        },
        16, values);
  }

  /**
   * @param {!mojo.internal.StructSpec} structSpec
   * @param {number} offset
   * @param {!Object} value
   */
  encodeStruct(structSpec, offset, value) {
    const structData = this.message_.allocate(structSpec.packedSize);
    const structEncoder =
        new mojo.internal.Encoder(this.message_, structData, this.context_);
    this.encodeOffset(offset, structData.byteOffset);
    structEncoder.encodeStructInline(structSpec, value);
  }

  /**
   * @param {!mojo.internal.StructSpec} structSpec
   * @param {!Object} value
   */
  encodeStructInline(structSpec, value) {
    const versions = structSpec.versions;
    this.encodeUint32(0, structSpec.packedSize);
    this.encodeUint32(4, versions[versions.length - 1].version);
    for (const field of structSpec.fields) {
      const byteOffset = mojo.internal.kStructHeaderSize + field.packedOffset;

      const encodeStructField = (field_value) => {
        field.type.$.encode(field_value, this, byteOffset,
                            field.packedBitOffset, field.nullable);
      };

      if (value && !mojo.internal.isNullOrUndefined(value[field.name])) {
        encodeStructField(value[field.name]);
        continue;
      }

      if (field.defaultValue !== null) {
        encodeStructField(field.defaultValue);
        continue;
      }

      if (field.nullable) {
        field.type.$.encodeNull(this, byteOffset);
        continue;
      }

      throw new Error(
        structSpec.name + ' missing value for non-nullable ' +
        'field "' + field.name + '"');
    }
  }

  /**
   * @param {!mojo.internal.UnionSpec} unionSpec
   * @param {number} offset
   * @param {!Object} value
   */
  encodeUnionAsPointer(unionSpec, offset, value) {
    const unionData = this.message_.allocate(mojo.internal.kUnionDataSize);
    const unionEncoder =
        new mojo.internal.Encoder(this.message_, unionData, this.context_);
    this.encodeOffset(offset, unionData.byteOffset);
    unionEncoder.encodeUnion(unionSpec, /*offset=*/0, value);
  }

  /**
   * @param {!mojo.internal.UnionSpec} unionSpec
   * @param {number} offset
   * @param {!Object} value
   */
  encodeUnion(unionSpec, offset, value) {
    const keys = Object.keys(value);
    if (keys.length !== 1) {
      throw new Error(
          `Value for ${unionSpec.name} must be an Object with a ` +
          'single property named one of: ' +
          Object.keys(unionSpec.fields).join(','));
    }

    const tag = keys[0];
    const field = unionSpec.fields[tag];
    this.encodeUint32(offset, mojo.internal.kUnionDataSize);
    this.encodeUint32(offset + 4, field['ordinal']);
    const fieldByteOffset = offset + mojo.internal.kUnionHeaderSize;
    if (typeof field['type'].$.unionSpec !== 'undefined') {
      // Unions are encoded as pointers when inside unions.
      this.encodeUnionAsPointer(field['type'].$.unionSpec,
                                fieldByteOffset,
                                value[tag]);
      return;
    }
    field['type'].$.encode(
        value[tag], this, fieldByteOffset, 0, field['nullable']);
  }

  /**
   * @param {string} value
   * @return {!Uint8Array}
   */
  static stringToUtf8Bytes(value) {
    if (!mojo.internal.Encoder.textEncoder)
      mojo.internal.Encoder.textEncoder = new TextEncoder('utf-8');
    return mojo.internal.Encoder.textEncoder.encode(value);
  }
};

/** @type {TextEncoder} */
mojo.internal.Encoder.textEncoder = null;

/**
 * Helps decode incoming messages. Decoders may be created recursively to
 * decode partial message fragments indexed by indirect message offsets, as with
 * encoded arrays and nested structs.
 */
mojo.internal.Decoder = class {
  /**
   * @param {!DataView} data
   * @param {!Array<MojoHandle>} handles
   * @param {?mojo.internal.MessageContext=} context
   */
  constructor(data, handles, context = null) {
    /** @private {?mojo.internal.MessageContext} */
    this.context_ = context;

    /** @private {!DataView} */
    this.data_ = data;

    /** @private {!Array<MojoHandle>} */
    this.handles_ = handles;
  }

  decodeBool(byteOffset, bitOffset) {
    return !!(this.data_.getUint8(byteOffset) & (1 << bitOffset));
  }

  decodeInt8(offset) {
    return this.data_.getInt8(offset);
  }

  decodeUint8(offset) {
    return this.data_.getUint8(offset);
  }

  decodeInt16(offset) {
    return this.data_.getInt16(offset, mojo.internal.kHostLittleEndian);
  }

  decodeUint16(offset) {
    return this.data_.getUint16(offset, mojo.internal.kHostLittleEndian);
  }

  decodeInt32(offset) {
    return this.data_.getInt32(offset, mojo.internal.kHostLittleEndian);
  }

  decodeUint32(offset) {
    return this.data_.getUint32(offset, mojo.internal.kHostLittleEndian);
  }

  decodeInt64(offset) {
    return mojo.internal.getInt64(this.data_, offset);
  }

  decodeUint64(offset) {
    return mojo.internal.getUint64(this.data_, offset);
  }

  decodeFloat(offset) {
    return this.data_.getFloat32(offset, mojo.internal.kHostLittleEndian);
  }

  decodeDouble(offset) {
    return this.data_.getFloat64(offset, mojo.internal.kHostLittleEndian);
  }

  decodeHandle(offset) {
    const index = this.data_.getUint32(offset, mojo.internal.kHostLittleEndian);
    if (index == 0xffffffff)
      return null;
    if (index >= this.handles_.length)
      throw new Error('Decoded invalid handle index');
    return this.handles_[index];
  }

  decodeString(offset) {
    const data = this.decodeArray({elementType: mojo.internal.Uint8}, offset);
    if (!data)
      return null;

    if (!mojo.internal.Decoder.textDecoder)
      mojo.internal.Decoder.textDecoder = new TextDecoder('utf-8');
    return mojo.internal.Decoder.textDecoder.decode(
        new Uint8Array(data).buffer);
  }

  decodeOffset(offset) {
    const relativeOffset = this.decodeUint64(offset);
    if (relativeOffset == 0)
      return 0;
    if (relativeOffset > BigInt(Number.MAX_SAFE_INTEGER))
      throw new Error('Mesage offset too large');
    return this.data_.byteOffset + offset + Number(relativeOffset);
  }

  /**
   * @param {!mojo.internal.ArraySpec} arraySpec
   * @return {Array}
   */
  decodeArray(arraySpec, offset) {
    const arrayOffset = this.decodeOffset(offset);
    if (!arrayOffset)
      return null;

    const arrayDecoder = new mojo.internal.Decoder(
        new DataView(this.data_.buffer, arrayOffset), this.handles_,
        this.context_);

    const size = arrayDecoder.decodeUint32(0);
    const numElements = arrayDecoder.decodeUint32(4);
    if (!numElements)
      return [];

    const result = [];
    if (arraySpec.elementType === mojo.internal.Bool) {
      for (let i = 0; i < numElements; ++i)
        result.push(arrayDecoder.decodeBool(8 + (i >> 3), i % 8));
    } else {
      let byteOffset = 8;
      for (let i = 0; i < numElements; ++i) {
        const element = arraySpec.elementType.$.decode(
            arrayDecoder, byteOffset, 0, !!arraySpec.elementNullable);
        if (element === null && !arraySpec.elementNullable)
          throw new Error('Received unexpected array element');
        result.push(element);
        byteOffset += arraySpec.elementType.$.arrayElementSize(
            !!arraySpec.elementNullable);
      }
    }
    return result;
  }

  /**
   * @param {!mojo.internal.MapSpec} mapSpec
   * @return {Object|Map}
   */
  decodeMap(mapSpec, offset) {
    const mapOffset = this.decodeOffset(offset);
    if (!mapOffset)
      return null;

    const mapDecoder = new mojo.internal.Decoder(
        new DataView(this.data_.buffer, mapOffset), this.handles_,
        this.context_);
    const mapStructSize = mapDecoder.decodeUint32(0);
    const mapStructVersion = mapDecoder.decodeUint32(4);
    if (mapStructSize != mojo.internal.kMapDataSize || mapStructVersion != 0)
      throw new Error('Received invalid map data');

    const keys = mapDecoder.decodeArray({elementType: mapSpec.keyType}, 8);
    const values = mapDecoder.decodeArray(
        {
          elementType: mapSpec.valueType,
          elementNullable: mapSpec.valueNullable
        },
        16);

    if (keys.length != values.length)
      throw new Error('Received invalid map data');
    if (!mapSpec.keyType.$.isValidObjectKeyType) {
      const map = new Map;
      for (let i = 0; i < keys.length; ++i)
        map.set(keys[i], values[i]);
      return map;
    }

    const map = {};
    for (let i = 0; i < keys.length; ++i)
      map[keys[i]] = values[i];
    return map;
  }

  /**
   * @param {!mojo.internal.StructSpec} structSpec
   * @return {Object}
   */
  decodeStruct(structSpec, offset) {
    const structOffset = this.decodeOffset(offset);
    if (!structOffset)
      return null;

    const decoder = new mojo.internal.Decoder(
        new DataView(this.data_.buffer, structOffset), this.handles_,
        this.context_);
    return decoder.decodeStructInline(structSpec);
  }

  /**
   * @param {!mojo.internal.StructSpec} structSpec
   * @param {number} size
   * @param {number} version
   * @return {boolean}
   */
  isStructHeaderValid(structSpec, size, version) {
    const versions = structSpec.versions;
    for (let i = versions.length - 1; i >= 0; --i) {
      const info = versions[i];
      if (version > info.version) {
        // If it's newer than the next newest version we know about, the only
        // requirement is that it's at least large enough to decode that next
        // newest version.
        return size >= info.packedSize;
      }
      if (version == info.version) {
        // If it IS the next newest version we know about, expect an exact size
        // match.
        return size == info.packedSize;
      }
    }

    // This should be effectively unreachable, because we always generate info
    // for version 0, and the `version` parameter here is guaranteed in practice
    // to be a non-negative value.
    throw new Error(
        `Impossible version ${version} for struct ${structSpec.name}`);
  }

  /**
   * @param {!mojo.internal.StructSpec} structSpec
   * @return {!Object}
   */
  decodeStructInline(structSpec) {
    const size = this.decodeUint32(0);
    const version = this.decodeUint32(4);
    if (!this.isStructHeaderValid(structSpec, size, version)) {
      throw new Error(
          `Received ${structSpec.name} of invalid size (${size}) and/or ` +
          `version (${version})`);
    }

    const result = {};
    for (const field of structSpec.fields) {
      const byteOffset = mojo.internal.kStructHeaderSize + field.packedOffset;
      if (field.minVersion > version) {
        result[field.name] = field.defaultValue;
        continue;
      }
      const value = field.type.$.decode(
          this, byteOffset, field.packedBitOffset, !!field.nullable);
      if (value === null && !field.nullable) {
        throw new Error(
            'Received ' + structSpec.name + ' with invalid null field ' +
            '"' + field.name + '"')
      }
      result[field.name] = value;
    }
    return result;
  }

  /**
   * @param {!mojo.internal.UnionSpec} unionSpec
   * @param {number} offset
   */
  decodeUnionFromPointer(unionSpec, offset) {
    const unionOffset = this.decodeOffset(offset);
    if (!unionOffset)
      return null;

    const decoder = new mojo.internal.Decoder(
        new DataView(this.data_.buffer, unionOffset), this.handles_,
        this.context_);
    return decoder.decodeUnion(unionSpec, 0);
  }

  /**
   * @param {!mojo.internal.UnionSpec} unionSpec
   * @param {number} offset
   */
  decodeUnion(unionSpec, offset) {
    const size = this.decodeUint32(offset);
    if (size === 0)
      return null;

    const ordinal = this.decodeUint32(offset + 4);
    for (const fieldName in unionSpec.fields) {
      const field = unionSpec.fields[fieldName];
      if (field['ordinal'] === ordinal) {
        const fieldValue = (() => {
          const fieldByteOffset = offset + mojo.internal.kUnionHeaderSize;
          // Unions are encoded as pointers when inside other
          // unions.
          if (typeof field['type'].$.unionSpec !== 'undefined') {
            return this.decodeUnionFromPointer(
              field['type'].$.unionSpec, fieldByteOffset);
          }
          return field['type'].$.decode(
            this, fieldByteOffset, 0, field['nullable'])
        })();

        if (fieldValue === null && !field['nullable']) {
          throw new Error(
              `Received ${unionSpec.name} with invalid null ` +
              `field: ${field['name']}`);
        }
        const value = {};
        value[fieldName] = fieldValue;
        return value;
      }
    }
  }

  decodeInterfaceProxy(type, offset) {
    const handle = this.decodeHandle(offset);
    const version = this.decodeUint32(offset + 4);  // TODO: support versioning
    if (!handle)
      return null;
    return new type(handle);
  }

  decodeInterfaceRequest(type, offset) {
    const handle = this.decodeHandle(offset);
    if (!handle)
      return null;
    return new type(mojo.internal.interfaceSupport.createEndpoint(handle));
  }

  decodeAssociatedEndpoint(offset) {
    if (!this.context_ || !this.context_.endpoint) {
      throw new Error('cannot deserialize associated endpoint without context');
    }
    const receivingEndpoint = this.context_.endpoint;
    const message = new DataView(this.data_.buffer);
    const interfaceIdsOffset = Number(mojo.internal.getUint64(message, 40));
    const numInterfaceIds = message.getUint32(
        interfaceIdsOffset + 44, mojo.internal.kHostLittleEndian);
    const interfaceIds = new Uint32Array(
        message.buffer,
        interfaceIdsOffset + mojo.internal.kArrayHeaderSize + 40,
        numInterfaceIds);
    const index = this.decodeUint32(offset);
    const interfaceId = interfaceIds[index];
    return new mojo.internal.interfaceSupport.Endpoint(
        receivingEndpoint.router, interfaceId);
  }
};

/** @type {TextDecoder} */
mojo.internal.Decoder.textDecoder = null;

/**
 * @typedef {{
 *   headerSize: number,
 *   headerVersion: number,
 *   interfaceId: number,
 *   ordinal: number,
 *   flags: number,
 *   requestId: number,
 * }}
 */
mojo.internal.MessageHeader;

/**
 * @param {!DataView} data
 * @return {!mojo.internal.MessageHeader}
 */
mojo.internal.deserializeMessageHeader = function(data) {
  const headerSize = data.getUint32(0, mojo.internal.kHostLittleEndian);
  const headerVersion = data.getUint32(4, mojo.internal.kHostLittleEndian);
  if ((headerVersion == 0 &&
       headerSize != mojo.internal.kMessageV0HeaderSize) ||
      (headerVersion == 1 &&
       headerSize != mojo.internal.kMessageV1HeaderSize) ||
      headerVersion > 2) {
    throw new Error('Received invalid message header');
  }
  return {
    headerSize,
    headerVersion,
    interfaceId: data.getUint32(8, mojo.internal.kHostLittleEndian),
    ordinal: data.getUint32(12, mojo.internal.kHostLittleEndian),
    flags: data.getUint32(16, mojo.internal.kHostLittleEndian),
    requestId: (headerVersion < 1) ?
        0 :
        data.getUint32(24, mojo.internal.kHostLittleEndian),
  };
};

/**
 * @typedef {{
 *   encode: function(*, !mojo.internal.Encoder, number, number, boolean),
 *   encodeNull: ((function(!mojo.internal.Encoder, number))|undefined),
 *   decode: function(!mojo.internal.Decoder, number, number, boolean):*,
 *   computeDimensions:
 *       ((function(*, boolean):!mojo.internal.MessageDimensions)|undefined),
 *   isValidObjectKeyType: boolean,
 *   hasInterfaceId: (boolean|undefined),
 *   arrayElementSize: ((function(boolean):number)|undefined),
 *   arraySpec: (!mojo.internal.ArraySpec|undefined),
 *   mapSpec: (!mojo.internal.MapSpec|undefined),
 *   structSpec: (!mojo.internal.StructSpec|undefined),
 * }}
 */
mojo.internal.MojomTypeInfo;

/**
 * @typedef {{
 *   $: !mojo.internal.MojomTypeInfo
 * }}
 */
mojo.internal.MojomType;

/**
 * @typedef {{
 *   elementType: !mojo.internal.MojomType,
 *   elementNullable: (boolean|undefined)
 * }}
 */
mojo.internal.ArraySpec;

/**
 * @typedef {{
 *   keyType: !mojo.internal.MojomType,
 *   valueType: !mojo.internal.MojomType,
 *   valueNullable: boolean
 * }}
 */
mojo.internal.MapSpec;

/**
 * @typedef {{
 *   name: string,
 *   packedOffset: number,
 *   packedBitOffset: number,
 *   type: !mojo.internal.MojomType,
 *   defaultValue: *,
 *   nullable: boolean,
 *   minVersion: number,
 * }}
 */
mojo.internal.StructFieldSpec;

/**
 * @typedef {{
 *   version: number,
 *   packedSize: number,
 * }}
 */
mojo.internal.StructVersionInfo;

/**
 * @typedef {{
 *   name: string,
 *   packedSize: number,
 *   fields: !Array<!mojo.internal.StructFieldSpec>,
 *   versions: !Array<!mojo.internal.StructVersionInfo>,
 * }}
 */
mojo.internal.StructSpec;

/**
 * @typedef {{
 *   name: string,
 *   ordinal: number,
 *   nullable: boolean
 * }}
 */
mojo.internal.UnionFieldSpec;

/**
 * @typedef {{
 *   name: string,
 *   fields: !Object<string, !mojo.internal.UnionFieldSpec>
 * }}
 */
mojo.internal.UnionSpec;

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Bool = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeBool(byteOffset, bitOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeBool(byteOffset, bitOffset);
    },
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Int8 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeInt8(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeInt8(byteOffset);
    },
    arrayElementSize: nullable => 1,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Uint8 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeUint8(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeUint8(byteOffset);
    },
    arrayElementSize: nullable => 1,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Int16 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeInt16(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeInt16(byteOffset);
    },
    arrayElementSize: nullable => 2,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Uint16 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeUint16(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeUint16(byteOffset);
    },
    arrayElementSize: nullable => 2,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Int32 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeInt32(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeInt32(byteOffset);
    },
    arrayElementSize: nullable => 4,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Uint32 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeUint32(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeUint32(byteOffset);
    },
    arrayElementSize: nullable => 4,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Int64 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeInt64(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeInt64(byteOffset);
    },
    arrayElementSize: nullable => 8,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Uint64 = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeUint64(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeUint64(byteOffset);
    },
    arrayElementSize: nullable => 8,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Float = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeFloat(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeFloat(byteOffset);
    },
    arrayElementSize: nullable => 4,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Double = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeDouble(byteOffset, value);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeDouble(byteOffset);
    },
    arrayElementSize: nullable => 8,
    isValidObjectKeyType: true,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Handle = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeHandle(byteOffset, value);
    },
    encodeNull: function(encoder, byteOffset) {
      encoder.encodeUint32(byteOffset, 0xffffffff);
    },
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeHandle(byteOffset);
    },
    arrayElementSize: nullable => 4,
    isValidObjectKeyType: false,
  },
};

/**
 * @const {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.String = {
  $: {
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeString(byteOffset, value);
    },
    encodeNull: function(encoder, byteOffset) {},
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeString(byteOffset);
    },
    computeDimensions: function(value, nullable) {
      const size = mojo.internal.computeTotalArraySize(
          {elementType: mojo.internal.Uint8},
          mojo.internal.Encoder.stringToUtf8Bytes(value));
      return {size};
    },
    arrayElementSize: nullable => 8,
    isValidObjectKeyType: true,
  }
};

/**
 * @param {!mojo.internal.MojomType} elementType
 * @param {boolean} elementNullable
 * @return {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Array = function(elementType, elementNullable) {
  /** @type {!mojo.internal.ArraySpec} */
  const arraySpec = {
    elementType: elementType,
    elementNullable: elementNullable,
  };
  return {
    $: {
      arraySpec: arraySpec,
      encode: function(value, encoder, byteOffset, bitOffset, nullable) {
        encoder.encodeArray(arraySpec, byteOffset, value);
      },
      encodeNull: function(encoder, byteOffset) {},
      decode: function(decoder, byteOffset, bitOffset, nullable) {
        return decoder.decodeArray(arraySpec, byteOffset);
      },
      computeDimensions: function(value, nullable) {
        return {size: mojo.internal.computeTotalArraySize(arraySpec, value)};
      },
      arrayElementSize: nullable => 8,
      isValidObjectKeyType: false,
    },
  };
};

/**
 * @param {!mojo.internal.MojomType} keyType
 * @param {!mojo.internal.MojomType} valueType
 * @param {boolean} valueNullable
 * @return {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Map = function(keyType, valueType, valueNullable) {
  /** @type {!mojo.internal.MapSpec} */
  const mapSpec = {
    keyType: keyType,
    valueType: valueType,
    valueNullable: valueNullable,
  };
  return {
    $: {
      mapSpec: mapSpec,
      encode: function(value, encoder, byteOffset, bitOffset, nullable) {
        encoder.encodeMap(mapSpec, byteOffset, value);
      },
      encodeNull: function(encoder, byteOffset) {},
      decode: function(decoder, byteOffset, bitOffset, nullable) {
        return decoder.decodeMap(mapSpec, byteOffset);
      },
      computeDimensions: function(value, nullable) {
        const keys =
            (value.constructor.name == 'Map') ? Array.from(value.keys())
                                              : Object.keys(value);
        const values =
            (value.constructor.name == 'Map') ? Array.from(value.values())
                                              : keys.map(k => value[k]);

        const size = mojo.internal.kMapDataSize +
            mojo.internal.computeTotalArraySize({elementType: keyType}, keys) +
            mojo.internal.computeTotalArraySize(
                {
                  elementType: valueType,
                  elementNullable: valueNullable,
                },
                values);
        return {size};
      },
      arrayElementSize: nullable => 8,
      isValidObjectKeyType: false,
    },
  };
};

/**
 * @return {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.Enum = function() {
  return {
    $: {
      encode: function(value, encoder, byteOffset, bitOffset, nullable) {
        // TODO: Do some sender-side error checking on the input value.
        encoder.encodeUint32(byteOffset, value);
      },
      decode: function(decoder, byteOffset, bitOffset, nullable) {
        const value = decoder.decodeInt32(byteOffset);
        // TODO: validate
        return value;
      },
      arrayElementSize: nullable => 4,
      isValidObjectKeyType: true,
    },
  };
};

/**
 * @param {string} name
 * @param {number} packedOffset
 * @param {number} packedBitOffset
 * @param {!mojo.internal.MojomType} type
 * @param {*} defaultValue
 * @param {boolean} nullable
 * @param {number=} minVersion
 * @return {!mojo.internal.StructFieldSpec}
 * @export
 */
mojo.internal.StructField = function(
    name, packedOffset, packedBitOffset, type, defaultValue, nullable,
    minVersion = 0) {
  return {
    name: name,
    packedOffset: packedOffset,
    packedBitOffset: packedBitOffset,
    type: type,
    defaultValue: defaultValue,
    nullable: nullable,
    minVersion: minVersion,
  };
};

/**
 * @param {!Object} objectToBlessAsType
 * @param {string} name
 * @param {!Array<!mojo.internal.StructFieldSpec>} fields
 * @param {Array<!Array<number>>=} versionData
 * @export
 */
mojo.internal.Struct = function(
    objectToBlessAsType, name, fields, versionData) {
  const versions = versionData.map(v => ({version: v[0], packedSize: v[1]}));
  const packedSize = versions[versions.length - 1].packedSize;
  const structSpec = {name, packedSize, fields, versions};
  objectToBlessAsType.$ = {
    structSpec: structSpec,
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeStruct(structSpec, byteOffset, value);
    },
    encodeNull: function(encoder, byteOffset) {},
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeStruct(structSpec, byteOffset);
    },
    computeDimensions: function(value, nullable) {
      return mojo.internal.computeStructDimensions(structSpec, value);
    },
    arrayElementSize: nullable => 8,
    isValidObjectKeyType: false,
  };
};

/**
 * @param {!mojo.internal.MojomType} structMojomType
 * @return {!Function}
 * @export
 */
mojo.internal.createStructDeserializer = function(structMojomType) {
  return function(dataView) {
    if (structMojomType.$ == undefined ||
        structMojomType.$.structSpec == undefined) {
      throw new Error('Invalid struct mojom type!');
    }
    const decoder = new mojo.internal.Decoder(dataView, []);
    return decoder.decodeStructInline(structMojomType.$.structSpec);
  };
};

/**
 * @param {!Object} objectToBlessAsUnion
 * @param {string} name
 * @param {!Object} fields
 * @export
 */
mojo.internal.Union = function(objectToBlessAsUnion, name, fields) {
  /** @type {!mojo.internal.UnionSpec} */
  const unionSpec = {
    name: name,
    fields: fields,
  };
  objectToBlessAsUnion.$ = {
    unionSpec: unionSpec,
    encode: function(value, encoder, byteOffset, bitOffset, nullable) {
      encoder.encodeUnion(unionSpec, byteOffset, value);
    },
    encodeNull: function(encoder, byteOffset) {},
    decode: function(decoder, byteOffset, bitOffset, nullable) {
      return decoder.decodeUnion(unionSpec, byteOffset);
    },
    computeDimensions: function(value, nullable) {
      return mojo.internal.computeUnionDimensions(unionSpec, nullable, value);
    },
    arrayElementSize: nullable => (nullable ? 8 : 16),
    isValidObjectKeyType: false,
  };
};

/**
 * @return {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.InterfaceProxy = function(type) {
  return {
    $: {
      encode: function(value, encoder, byteOffset, bitOffset, nullable) {
        const endpoint = value.proxy.unbind();
        console.assert(endpoint, `unexpected null ${type.name}`);

        const pipe = endpoint.releasePipe();
        encoder.encodeHandle(byteOffset, pipe);
        encoder.encodeUint32(byteOffset + 4, 0);  // TODO: Support versioning
      },
      encodeNull: function(encoder, byteOffset) {
        encoder.encodeUint32(byteOffset, 0xffffffff);
      },
      decode: function(decoder, byteOffset, bitOffset, nullable) {
        return decoder.decodeInterfaceProxy(type, byteOffset);
      },
      arrayElementSize: nullable => 8,
      isValidObjectKeyType: false,
    },
  };
};

/**
 * @return {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.InterfaceRequest = function(type) {
  return {
    $: {
      encode: function(value, encoder, byteOffset, bitOffset, nullable) {
        if (!value.handle)
          throw new Error('Unexpected null ' + type.name);

        encoder.encodeHandle(byteOffset, value.handle.releasePipe());
      },
      encodeNull: function(encoder, byteOffset) {
        encoder.encodeUint32(byteOffset, 0xffffffff);
      },
      decode: function(decoder, byteOffset, bitOffset, nullable) {
        return decoder.decodeInterfaceRequest(type, byteOffset);
      },
      arrayElementSize: nullable => 8,
      isValidObjectKeyType: false,
    },
  };
};

/**
 * @return {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.AssociatedInterfaceProxy = function(type) {
  return {
    $: {
      type: type,
      encode: function(value, encoder, byteOffset, bitOffset, nullable) {
        console.assert(
            value.proxy.endpoint && value.proxy.endpoint.isPendingAssociation,
            `expected ${type.name} to be associated and unbound`);
        encoder.encodeAssociatedEndpoint(byteOffset, value.proxy.endpoint);
        encoder.encodeUint32(byteOffset + 4, 0);
      },
      encodeNull: function(encoder, byteOffset) {
        encoder.encodeUint32(byteOffset, 0xffffffff);
        encoder.encodeUint32(byteOffset + 4, 0);
      },
      decode: function(decoder, byteOffset, bitOffset, nullable) {
        return new type(decoder.decodeAssociatedEndpoint(byteOffset));
      },
      arrayElementSize: _ => {
        throw new Error('Arrays of associated endpoints are not yet supported');
      },
      isValidObjectKeyType: false,
      hasInterfaceId: true,
    },
  };
};

/**
 * @return {!mojo.internal.MojomType}
 * @export
 */
mojo.internal.AssociatedInterfaceRequest = function(type) {
  return {
    $: {
      type: type,
      encode: function(value, encoder, byteOffset, bitOffset, nullable) {
        console.assert(
            value.handle && value.handle.isPendingAssociation,
            `expected ${type.name} to be associated and unbound`);

        encoder.encodeAssociatedEndpoint(byteOffset, value.handle);
      },
      encodeNull: function(encoder, byteOffset) {
        encoder.encodeUint32(byteOffset, 0xffffffff);
      },
      decode: function(decoder, byteOffset, bitOffset, nullable) {
        return new type(decoder.decodeAssociatedEndpoint(byteOffset));
      },
      arrayElementSize: _ => {
        throw new Error('Arrays of associated endpoints are not yet supported');
      },
      isValidObjectKeyType: false,
      hasInterfaceId: true,
    },
  };
};
// mojo/public/interfaces/bindings/interface_control_messages.mojom-lite.js is auto generated by mojom_bindings_generator.py, do not edit







/**
 * @const { !number }
 * @export
 */
mojo.interfaceControl.RUN_MESSAGE_ID = 0xFFFFFFFF;

/**
 * @const { !number }
 * @export
 */
mojo.interfaceControl.RUN_OR_CLOSE_PIPE_MESSAGE_ID = 0xFFFFFFFE;




/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.RunMessageParamsSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.RunResponseMessageParamsSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.QueryVersionSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.QueryVersionResultSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.FlushForTestingSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.RunOrClosePipeMessageParamsSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.RequireVersionSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.EnableIdleTrackingSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.MessageAckSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.interfaceControl.NotifyIdleSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType} }
 * @export
 */
mojo.interfaceControl.RunInputSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType} }
 * @export
 */
mojo.interfaceControl.RunOutputSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType} }
 * @export
 */
mojo.interfaceControl.RunOrClosePipeInputSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };




mojo.internal.Struct(
    mojo.interfaceControl.RunMessageParamsSpec.$,
    'RunMessageParams',
    [
      mojo.internal.StructField(
        'input', 0,
        0,
        mojo.interfaceControl.RunInputSpec.$,
        null,
        false /* nullable */),
    ],
    [[0, 24],]);





/** @record */
mojo.interfaceControl.RunMessageParams = class {
  constructor() {
    /** @export { !mojo.interfaceControl.RunInput } */
    this.input;
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.RunResponseMessageParamsSpec.$,
    'RunResponseMessageParams',
    [
      mojo.internal.StructField(
        'output', 0,
        0,
        mojo.interfaceControl.RunOutputSpec.$,
        null,
        true /* nullable */),
    ],
    [[0, 24],]);





/** @record */
mojo.interfaceControl.RunResponseMessageParams = class {
  constructor() {
    /** @export { (mojo.interfaceControl.RunOutput|undefined) } */
    this.output;
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.QueryVersionSpec.$,
    'QueryVersion',
    [
    ],
    [[0, 8],]);





/** @record */
mojo.interfaceControl.QueryVersion = class {
  constructor() {
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.QueryVersionResultSpec.$,
    'QueryVersionResult',
    [
      mojo.internal.StructField(
        'version', 0,
        0,
        mojo.internal.Uint32,
        0,
        false /* nullable */),
    ],
    [[0, 16],]);





/** @record */
mojo.interfaceControl.QueryVersionResult = class {
  constructor() {
    /** @export { !number } */
    this.version;
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.FlushForTestingSpec.$,
    'FlushForTesting',
    [
    ],
    [[0, 8],]);





/** @record */
mojo.interfaceControl.FlushForTesting = class {
  constructor() {
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.RunOrClosePipeMessageParamsSpec.$,
    'RunOrClosePipeMessageParams',
    [
      mojo.internal.StructField(
        'input', 0,
        0,
        mojo.interfaceControl.RunOrClosePipeInputSpec.$,
        null,
        false /* nullable */),
    ],
    [[0, 24],]);





/** @record */
mojo.interfaceControl.RunOrClosePipeMessageParams = class {
  constructor() {
    /** @export { !mojo.interfaceControl.RunOrClosePipeInput } */
    this.input;
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.RequireVersionSpec.$,
    'RequireVersion',
    [
      mojo.internal.StructField(
        'version', 0,
        0,
        mojo.internal.Uint32,
        0,
        false /* nullable */),
    ],
    [[0, 16],]);





/** @record */
mojo.interfaceControl.RequireVersion = class {
  constructor() {
    /** @export { !number } */
    this.version;
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.EnableIdleTrackingSpec.$,
    'EnableIdleTracking',
    [
      mojo.internal.StructField(
        'timeoutInMicroseconds', 0,
        0,
        mojo.internal.Int64,
        BigInt(0),
        false /* nullable */),
    ],
    [[0, 16],]);





/** @record */
mojo.interfaceControl.EnableIdleTracking = class {
  constructor() {
    /** @export { !bigint } */
    this.timeoutInMicroseconds;
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.MessageAckSpec.$,
    'MessageAck',
    [
    ],
    [[0, 8],]);





/** @record */
mojo.interfaceControl.MessageAck = class {
  constructor() {
  }
};




mojo.internal.Struct(
    mojo.interfaceControl.NotifyIdleSpec.$,
    'NotifyIdle',
    [
    ],
    [[0, 8],]);





/** @record */
mojo.interfaceControl.NotifyIdle = class {
  constructor() {
  }
};




mojo.internal.Union(
    mojo.interfaceControl.RunInputSpec.$, 'RunInput',
    {
      'queryVersion': {
        'ordinal': 0,
        'type': mojo.interfaceControl.QueryVersionSpec.$,
      },
      'flushForTesting': {
        'ordinal': 1,
        'type': mojo.interfaceControl.FlushForTestingSpec.$,
      },
    });

/**
 * @typedef { {
 *   queryVersion: (!mojo.interfaceControl.QueryVersion|undefined),
 *   flushForTesting: (!mojo.interfaceControl.FlushForTesting|undefined),
 * } }
 */
mojo.interfaceControl.RunInput;


mojo.internal.Union(
    mojo.interfaceControl.RunOutputSpec.$, 'RunOutput',
    {
      'queryVersionResult': {
        'ordinal': 0,
        'type': mojo.interfaceControl.QueryVersionResultSpec.$,
      },
    });

/**
 * @typedef { {
 *   queryVersionResult: (!mojo.interfaceControl.QueryVersionResult|undefined),
 * } }
 */
mojo.interfaceControl.RunOutput;


mojo.internal.Union(
    mojo.interfaceControl.RunOrClosePipeInputSpec.$, 'RunOrClosePipeInput',
    {
      'requireVersion': {
        'ordinal': 0,
        'type': mojo.interfaceControl.RequireVersionSpec.$,
      },
      'enableIdleTracking': {
        'ordinal': 1,
        'type': mojo.interfaceControl.EnableIdleTrackingSpec.$,
      },
      'messageAck': {
        'ordinal': 2,
        'type': mojo.interfaceControl.MessageAckSpec.$,
      },
      'notifyIdle': {
        'ordinal': 3,
        'type': mojo.interfaceControl.NotifyIdleSpec.$,
      },
    });

/**
 * @typedef { {
 *   requireVersion: (!mojo.interfaceControl.RequireVersion|undefined),
 *   enableIdleTracking: (!mojo.interfaceControl.EnableIdleTracking|undefined),
 *   messageAck: (!mojo.interfaceControl.MessageAck|undefined),
 *   notifyIdle: (!mojo.interfaceControl.NotifyIdle|undefined),
 * } }
 */
mojo.interfaceControl.RunOrClosePipeInput;
// mojo/public/interfaces/bindings/pipe_control_messages.mojom-lite.js is auto generated by mojom_bindings_generator.py, do not edit







/**
 * @const { !number }
 * @export
 */
mojo.pipeControl.RUN_OR_CLOSE_PIPE_MESSAGE_ID = 0xFFFFFFFE;




/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.pipeControl.RunOrClosePipeMessageParamsSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.pipeControl.DisconnectReasonSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.pipeControl.PeerAssociatedEndpointClosedEventSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.pipeControl.PauseUntilFlushCompletesSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType}}
 * @export
 */
mojo.pipeControl.FlushAsyncSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };


/**
 * @const { {$:!mojo.internal.MojomType} }
 * @export
 */
mojo.pipeControl.RunOrClosePipeInputSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };




mojo.internal.Struct(
    mojo.pipeControl.RunOrClosePipeMessageParamsSpec.$,
    'RunOrClosePipeMessageParams',
    [
      mojo.internal.StructField(
        'input', 0,
        0,
        mojo.pipeControl.RunOrClosePipeInputSpec.$,
        null,
        false /* nullable */),
    ],
    [[0, 24],]);





/** @record */
mojo.pipeControl.RunOrClosePipeMessageParams = class {
  constructor() {
    /** @export { !mojo.pipeControl.RunOrClosePipeInput } */
    this.input;
  }
};




mojo.internal.Struct(
    mojo.pipeControl.DisconnectReasonSpec.$,
    'DisconnectReason',
    [
      mojo.internal.StructField(
        'customReason', 0,
        0,
        mojo.internal.Uint32,
        0,
        false /* nullable */),
      mojo.internal.StructField(
        'description', 8,
        0,
        mojo.internal.String,
        null,
        false /* nullable */),
    ],
    [[0, 24],]);





/** @record */
mojo.pipeControl.DisconnectReason = class {
  constructor() {
    /** @export { !number } */
    this.customReason;
    /** @export { !string } */
    this.description;
  }
};




mojo.internal.Struct(
    mojo.pipeControl.PeerAssociatedEndpointClosedEventSpec.$,
    'PeerAssociatedEndpointClosedEvent',
    [
      mojo.internal.StructField(
        'id', 0,
        0,
        mojo.internal.Uint32,
        0,
        false /* nullable */),
      mojo.internal.StructField(
        'disconnectReason', 8,
        0,
        mojo.pipeControl.DisconnectReasonSpec.$,
        null,
        true /* nullable */),
    ],
    [[0, 24],]);





/** @record */
mojo.pipeControl.PeerAssociatedEndpointClosedEvent = class {
  constructor() {
    /** @export { !number } */
    this.id;
    /** @export { (mojo.pipeControl.DisconnectReason|undefined) } */
    this.disconnectReason;
  }
};




mojo.internal.Struct(
    mojo.pipeControl.PauseUntilFlushCompletesSpec.$,
    'PauseUntilFlushCompletes',
    [
      mojo.internal.StructField(
        'flushPipe', 0,
        0,
        mojo.internal.Handle,
        null,
        false /* nullable */),
    ],
    [[0, 16],]);





/** @record */
mojo.pipeControl.PauseUntilFlushCompletes = class {
  constructor() {
    /** @export { !MojoHandle } */
    this.flushPipe;
  }
};




mojo.internal.Struct(
    mojo.pipeControl.FlushAsyncSpec.$,
    'FlushAsync',
    [
      mojo.internal.StructField(
        'flusherPipe', 0,
        0,
        mojo.internal.Handle,
        null,
        false /* nullable */),
    ],
    [[0, 16],]);





/** @record */
mojo.pipeControl.FlushAsync = class {
  constructor() {
    /** @export { !MojoHandle } */
    this.flusherPipe;
  }
};




mojo.internal.Union(
    mojo.pipeControl.RunOrClosePipeInputSpec.$, 'RunOrClosePipeInput',
    {
      'peerAssociatedEndpointClosedEvent': {
        'ordinal': 0,
        'type': mojo.pipeControl.PeerAssociatedEndpointClosedEventSpec.$,
      },
      'pauseUntilFlushCompletes': {
        'ordinal': 1,
        'type': mojo.pipeControl.PauseUntilFlushCompletesSpec.$,
      },
      'flushAsync': {
        'ordinal': 2,
        'type': mojo.pipeControl.FlushAsyncSpec.$,
      },
    });

/**
 * @typedef { {
 *   peerAssociatedEndpointClosedEvent: (!mojo.pipeControl.PeerAssociatedEndpointClosedEvent|undefined),
 *   pauseUntilFlushCompletes: (!mojo.pipeControl.PauseUntilFlushCompletes|undefined),
 *   flushAsync: (!mojo.pipeControl.FlushAsync|undefined),
 * } }
 */
mojo.pipeControl.RunOrClosePipeInput;
// Copyright 2018 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

/**
 * Owns a single message pipe handle and facilitates message sending and routing
 * on behalf of all the pipe's local Endpoints.
 */
mojo.internal.interfaceSupport.Router = class {
  /**
   * @param {!MojoHandle} pipe
   * @param {boolean} setNamespaceBit
   * @public
   */
  constructor(pipe, setNamespaceBit) {
    /** @const {!MojoHandle} */
    this.pipe_ = pipe;

    /** @const {!mojo.internal.interfaceSupport.HandleReader} */
    this.reader_ = new mojo.internal.interfaceSupport.HandleReader(pipe);
    this.reader_.onRead = this.onMessageReceived_.bind(this);
    this.reader_.onError = this.onError_.bind(this);

    /** @const {!Map<number, !mojo.internal.interfaceSupport.Endpoint>} */
    this.endpoints_ = new Map();

    /** @private {number} */
    this.nextInterfaceId_ = 1;

    /** @const {number} */
    this.interfaceIdNamespace_ =
        setNamespaceBit ? mojo.internal.kInterfaceNamespaceBit : 0;

    /** @const {!mojo.internal.interfaceSupport.PipeControlMessageHandler} */
    this.pipeControlHandler_ =
        new mojo.internal.interfaceSupport.PipeControlMessageHandler(
            this, this.onPeerEndpointClosed_.bind(this));
  }

  /** @return {!MojoHandle} */
  get pipe() {
    return this.pipe_;
  }

  /** @return {number} */
  generateInterfaceId() {
    return (this.nextInterfaceId_++ | this.interfaceIdNamespace_) >>> 0;
  }

  /**
   * @param {!mojo.internal.interfaceSupport.Endpoint} endpoint
   * @param {number} interfaceId
   */
  addEndpoint(endpoint, interfaceId) {
    if (interfaceId === 0) {
      this.reader_.start();
    }
    console.assert(
        this.isReading(), 'adding a secondary endpoint with no primary');
    this.endpoints_.set(interfaceId, endpoint);
  }

  /** @param {number} interfaceId */
  removeEndpoint(interfaceId) {
    this.endpoints_.delete(interfaceId);
    if (interfaceId === 0) {
      this.reader_.stop();
    }
  }

  close() {
    console.assert(
        this.endpoints_.size === 0,
        'closing primary endpoint with secondary endpoints still bound');
    this.reader_.stopAndCloseHandle();
  }

  /** @param {number} interfaceId */
  closeEndpoint(interfaceId) {
    this.removeEndpoint(interfaceId);
    if (interfaceId === 0) {
      this.close();
    } else {
      this.pipeControlHandler_.notifyEndpointClosed(interfaceId);
    }
  }

  /** @return {boolean} */
  isReading() {
    return !this.reader_.isStopped();
  }

  /** @param {!mojo.internal.Message} message */
  send(message) {
    this.pipe_.writeMessage(message.buffer, message.handles);
  }

  /**
   * @param {!ArrayBuffer} buffer
   * @param {!Array<MojoHandle>} handles
   */
  onMessageReceived_(buffer, handles) {
    if (buffer.byteLength < mojo.internal.kMessageV0HeaderSize) {
      console.error('Rejecting undersized message');
      this.onError_();
      return;
    }

    const header = mojo.internal.deserializeMessageHeader(new DataView(buffer));
    if (this.pipeControlHandler_.maybeHandleMessage(header, buffer)) {
      return;
    }

    const endpoint = this.endpoints_.get(header.interfaceId);
    if (!endpoint) {
      console.error(
          `Received message for unknown endpoint ${header.interfaceId}`);
      return;
    }

    endpoint.onMessageReceived(header, buffer, handles);
  }

  onError_() {
    for (const endpoint of this.endpoints_.values()) {
      endpoint.onError();
    }
    this.endpoints_.clear();
  }

  /** @param {number} id */
  onPeerEndpointClosed_(id) {
    const endpoint = this.endpoints_.get(id);
    if (endpoint) {
      endpoint.onError();
    }
  }
};

/**
 * Something which can receive notifications from an Endpoint; generally this is
 * the Endpoint's owner.
 * @interface
 */
mojo.internal.interfaceSupport.EndpointClient = class {
  /**
   * @param {!mojo.internal.interfaceSupport.Endpoint} endpoint
   * @param {!mojo.internal.MessageHeader} header
   * @param {!ArrayBuffer} buffer
   * @param {!Array<MojoHandle>} handles
   */
  onMessageReceived(endpoint, header, buffer, handles) {}

  /**
   * @param {!mojo.internal.interfaceSupport.Endpoint} endpoint
   * @param {string=} reason
   */
  onError(endpoint, reason = undefined) {}
};

/**
 * Encapsulates a single interface endpoint on a multiplexed Router object. This
 * may be the primary (possibly only) endpoint on a pipe, or a secondary
 * associated interface endpoint.
 */
mojo.internal.interfaceSupport.Endpoint = class {
  /**
   * @param {mojo.internal.interfaceSupport.Router=} router
   * @param {number=} interfaceId
   */
  constructor(router = null, interfaceId = 0) {
    /** @private {mojo.internal.interfaceSupport.Router} */
    this.router_ = router;

    /** @private {number} */
    this.interfaceId_ = interfaceId;

    /** @private {mojo.internal.interfaceSupport.ControlMessageHandler} */
    this.controlMessageHandler_ =
        new mojo.internal.interfaceSupport.ControlMessageHandler(this);

    /** @private {mojo.internal.interfaceSupport.EndpointClient} */
    this.client_ = null;

    /** @private {number} */
    this.nextRequestId_ = 0;

    /** @private {mojo.internal.interfaceSupport.Endpoint} */
    this.localPeer_ = null;
  }

  /**
   * @return {{
   *   endpoint0: !mojo.internal.interfaceSupport.Endpoint,
   *   endpoint1: !mojo.internal.interfaceSupport.Endpoint,
   * }}
   */
  static createAssociatedPair() {
    const endpoint0 = new mojo.internal.interfaceSupport.Endpoint();
    const endpoint1 = new mojo.internal.interfaceSupport.Endpoint();
    endpoint1.localPeer_ = endpoint0;
    endpoint0.localPeer_ = endpoint1;
    return {endpoint0, endpoint1};
  }

  /** @return {mojo.internal.interfaceSupport.Router} */
  get router() {
    return this.router_;
  }

  /** @return {boolean} */
  isPrimary() {
    return this.router_ !== null && this.interfaceId_ === 0;
  }

  /** @return {!MojoHandle} */
  releasePipe() {
    console.assert(this.isPrimary(), 'secondary endpoint cannot release pipe');
    return this.router_.pipe;
  }

  /** @return {boolean} */
  get isPendingAssociation() {
    return this.localPeer_ !== null;
  }

  /**
   * @param {string} interfaceName
   * @param {string} scope
   */
  bindInBrowser(interfaceName, scope) {
    console.assert(
        this.isPrimary() && !this.router_.isReading(),
        'endpoint is either associated or already bound');
    Mojo.bindInterface(interfaceName, this.router_.pipe, scope);
  }

  /**
   * @param {!mojo.internal.interfaceSupport.Endpoint} endpoint
   * @return {number}
   */
  associatePeerOfOutgoingEndpoint(endpoint) {
    console.assert(this.router_, 'cannot associate with unbound endpoint');
    const peer = endpoint.localPeer_;
    endpoint.localPeer_ = peer.localPeer_ = null;

    const id = this.router_.generateInterfaceId();
    peer.router_ = this.router_;
    peer.interfaceId_ = id;
    if (peer.client_) {
      this.router_.addEndpoint(peer, id);
    }
    return id;
  }

  /** @return {number} */
  generateRequestId() {
    const id = this.nextRequestId_++;
    if (this.nextRequestId_ > 0xffffffff) {
      this.nextRequestId_ = 0;
    }
    return id;
  }

  /**
   * @param {number} ordinal
   * @param {number} requestId
   * @param {number} flags
   * @param {!mojo.internal.MojomType} paramStruct
   * @param {!Object} value
   */
  send(ordinal, requestId, flags, paramStruct, value) {
    const message = new mojo.internal.Message(
        this, this.interfaceId_, flags, ordinal, requestId,
        /** @type {!mojo.internal.StructSpec} */ (paramStruct.$.structSpec),
        value);
    console.assert(
        this.router_, 'cannot send message on unassociated unbound endpoint');
    this.router_.send(message);
  }

  /** @param {mojo.internal.interfaceSupport.EndpointClient} client */
  start(client) {
    console.assert(!this.client_, 'endpoint already started');
    this.client_ = client;
    if (this.router_) {
      this.router_.addEndpoint(this, this.interfaceId_);
    }
  }

  /** @return {boolean} */
  get isStarted() {
    return this.client_ !== null;
  }

  stop() {
    if (this.router_) {
      this.router_.removeEndpoint(this.interfaceId_);
    }
    this.client_ = null;
    this.controlMessageHandler_ = null;
  }

  close() {
    if (this.router_) {
      this.router.closeEndpoint(this.interfaceId_);
    }
    this.client_ = null;
    this.controlMessageHandler_ = null;
  }

  async flushForTesting() {
    return this.controlMessageHandler_.sendRunMessage({'flushForTesting': {}});
  }

  /**
   * @param {!mojo.internal.MessageHeader} header
   * @param {!ArrayBuffer} buffer
   * @param {!Array<MojoHandle>} handles
   */
  onMessageReceived(header, buffer, handles) {
    console.assert(this.client_, 'endpoint has no client');
    const handled =
        this.controlMessageHandler_.maybeHandleControlMessage(header, buffer);
    if (handled) {
      return;
    }

    this.client_.onMessageReceived(this, header, buffer, handles);
  }

  onError() {
    if (this.client_) {
      this.client_.onError(this);
    }
  }
};

/**
 * Creates a new Endpoint wrapping a given pipe handle.
 *
 * @param {!MojoHandle|!mojo.internal.interfaceSupport.Endpoint} pipeOrEndpoint
 * @param {boolean=} setNamespaceBit
 * @return {!mojo.internal.interfaceSupport.Endpoint}
 */
mojo.internal.interfaceSupport.createEndpoint = function(
    pipeOrEndpoint, setNamespaceBit = false) {
  if (pipeOrEndpoint.constructor.name != 'MojoHandle') {
    return /** @type {!mojo.internal.interfaceSupport.Endpoint} */(
        pipeOrEndpoint);
  }
  return new mojo.internal.interfaceSupport.Endpoint(
      new mojo.internal.interfaceSupport.Router(
          /** @type {!MojoHandle} */(pipeOrEndpoint), setNamespaceBit),
      0);
};

/**
 * Returns its input if given an existing Endpoint. If given a pipe handle,
 * creates a new Endpoint to own it and returns that. This is a helper for
 * generated PendingReceiver constructors since they can accept either type as
 * input.
 *
 * @param {!MojoHandle|!mojo.internal.interfaceSupport.Endpoint} handle
 * @return {!mojo.internal.interfaceSupport.Endpoint}
 * @export
 */
mojo.internal.interfaceSupport.getEndpointForReceiver = function(handle) {
  return mojo.internal.interfaceSupport.createEndpoint(handle);
};

/**
 * @param {!mojo.internal.interfaceSupport.Endpoint} endpoint
 * @param {string} interfaceName
 * @param {string} scope
 * @export
 */
mojo.internal.interfaceSupport.bind = function(endpoint, interfaceName, scope) {
  endpoint.bindInBrowser(interfaceName, scope);
};

mojo.internal.interfaceSupport.PipeControlMessageHandler = class {
  /**
   * @param {!mojo.internal.interfaceSupport.Router} router
   * @param {function(number)} onDisconnect
   */
  constructor(router, onDisconnect) {
    /** @const {!mojo.internal.interfaceSupport.Router} */
    this.router_ = router;

    /** @const {function(number)} */
    this.onDisconnect_ = onDisconnect;
  }

  /**
   * @param {!mojo.pipeControl.RunOrClosePipeInput} input
   */
  send(input) {
    const spec = /** @type {!mojo.internal.StructSpec} */ (
        mojo.pipeControl.RunOrClosePipeMessageParamsSpec.$.$.structSpec);
    const message = new mojo.internal.Message(
        null, 0xffffffff, 0, mojo.pipeControl.RUN_OR_CLOSE_PIPE_MESSAGE_ID, 0,
        /** @type {!mojo.internal.StructSpec} */
        (mojo.pipeControl.RunOrClosePipeMessageParamsSpec.$.$.structSpec),
        {'input': input});
    this.router_.send(message);
  }

  /**
   * @param {!mojo.internal.MessageHeader} header
   * @param {!ArrayBuffer} buffer
   * @return {boolean}
   */
  maybeHandleMessage(header, buffer) {
    if (header.ordinal !== mojo.pipeControl.RUN_OR_CLOSE_PIPE_MESSAGE_ID) {
      return false;
    }

    const data = new DataView(buffer, header.headerSize);
    const decoder = new mojo.internal.Decoder(data, []);
    const spec = /** @type {!mojo.internal.StructSpec} */ (
        mojo.pipeControl.RunOrClosePipeMessageParamsSpec.$.$.structSpec);
    const input = decoder.decodeStructInline(spec)['input'];
    if (input.hasOwnProperty('peerAssociatedEndpointClosedEvent')) {
      this.onDisconnect_(input['peerAssociatedEndpointClosedEvent']['id']);
      return true;
    }

    return true;
  }

  /**@param {number} interfaceId */
  notifyEndpointClosed(interfaceId) {
    this.send({'peerAssociatedEndpointClosedEvent': {'id': interfaceId}});
  }
};

/**
 * Handles incoming interface control messages on an interface endpoint.
 */
mojo.internal.interfaceSupport.ControlMessageHandler = class {
  /** @param {!mojo.internal.interfaceSupport.Endpoint} endpoint */
  constructor(endpoint) {
    /** @private {!mojo.internal.interfaceSupport.Endpoint} */
    this.endpoint_ = endpoint;

    /** @private {!Map<number, function()>} */
    this.pendingFlushResolvers_ = new Map;
  }

  sendRunMessage(input) {
    const requestId = this.endpoint_.generateRequestId();
    return new Promise(resolve => {
      this.endpoint_.send(
          mojo.interfaceControl.RUN_MESSAGE_ID, requestId,
          mojo.internal.kMessageFlagExpectsResponse,
          mojo.interfaceControl.RunMessageParamsSpec.$, {'input': input});
      this.pendingFlushResolvers_.set(requestId, resolve);
    });
  }

  maybeHandleControlMessage(header, buffer) {
    if (header.ordinal === mojo.interfaceControl.RUN_MESSAGE_ID) {
      const data = new DataView(buffer, header.headerSize);
      const decoder = new mojo.internal.Decoder(data, []);
      if (header.flags & mojo.internal.kMessageFlagExpectsResponse)
        return this.handleRunRequest_(header.requestId, decoder);
      else
        return this.handleRunResponse_(header.requestId, decoder);
    }

    return false;
  }

  handleRunRequest_(requestId, decoder) {
    const input = decoder.decodeStructInline(
        mojo.interfaceControl.RunMessageParamsSpec.$.$.structSpec)['input'];
    if (input.hasOwnProperty('flushForTesting')) {
      this.endpoint_.send(
          mojo.interfaceControl.RUN_MESSAGE_ID, requestId,
          mojo.internal.kMessageFlagIsResponse,
          mojo.interfaceControl.RunResponseMessageParamsSpec.$,
          {'output': null});
      return true;
    }

    return false;
  }

  handleRunResponse_(requestId, decoder) {
    const resolver = this.pendingFlushResolvers_.get(requestId);
    if (!resolver)
      return false;

    resolver();
    return true;
  }
};

/**
 * Captures metadata about a request which was sent by a remote, for which a
 * response is expected.
 *
 * @typedef {{
 *   requestId: number,
 *   ordinal: number,
 *   responseStruct: !mojo.internal.MojomType,
 *   resolve: !Function,
 *   reject: !Function,
 * }}
 */
mojo.internal.interfaceSupport.PendingResponse;

/**
 * Exposed by endpoints to allow observation of remote peer closure. Any number
 * of listeners may be registered on a ConnectionErrorEventRouter, and the
 * router will dispatch at most one event in its lifetime, whenever its endpoint
 * detects peer closure.
 * @export
 */
mojo.internal.interfaceSupport.ConnectionErrorEventRouter = class {
  /** @public */
  constructor() {
    /** @type {!Map<number, !Function>} */
    this.listeners = new Map;

    /** @private {number} */
    this.nextListenerId_ = 0;
  }

  /**
   * @param {!Function} listener
   * @return {number} An ID which can be given to removeListener() to remove
   *     this listener.
   * @export
   */
  addListener(listener) {
    const id = ++this.nextListenerId_;
    this.listeners.set(id, listener);
    return id;
  }

  /**
   * @param {number} id An ID returned by a prior call to addListener.
   * @return {boolean} True iff the identified listener was found and removed.
   * @export
   */
  removeListener(id) {
    return this.listeners.delete(id);
  }

  /**
   * Notifies all listeners of a connection error.
   */
  dispatchErrorEvent() {
    for (const listener of this.listeners.values())
      listener();
  }
};

/**
 * @interface
 * @export
 */
mojo.internal.interfaceSupport.PendingReceiver = class {
  /**
   * @return {!mojo.internal.interfaceSupport.Endpoint}
   * @export
   */
  get handle() {}
};

/**
 * Generic helper used to implement all generated remote classes. Knows how to
 * serialize requests and deserialize their replies, both according to
 * declarative message structure specs.
 *
 * TODO(crbug.com/1012109): Use a bounded generic type instead of
 * mojo.internal.interfaceSupport.PendingReceiver.
 * @implements {mojo.internal.interfaceSupport.EndpointClient}
 * @export
 */
mojo.internal.interfaceSupport.InterfaceRemoteBase = class {
  /**
   * @param {!function(new:mojo.internal.interfaceSupport.PendingReceiver,
   *     !mojo.internal.interfaceSupport.Endpoint)} requestType
   * @param {MojoHandle|mojo.internal.interfaceSupport.Endpoint=} handle
   *     The pipe or endpoint handle to use as a remote endpoint. If omitted,
   *     this object must be bound with bindHandle before it can be used to send
   *     messages.
   * @public
   */
  constructor(requestType, handle = undefined) {
    /** @private {mojo.internal.interfaceSupport.Endpoint} */
    this.endpoint_ = null;

    /**
     * @private {!function(new:mojo.internal.interfaceSupport.PendingReceiver,
     *     !mojo.internal.interfaceSupport.Endpoint)}
     */
    this.requestType_ = requestType;

    /**
     * @private {!Map<number, !mojo.internal.interfaceSupport.PendingResponse>}
     */
    this.pendingResponses_ = new Map;

    /** @const {!mojo.internal.interfaceSupport.ConnectionErrorEventRouter} */
    this.connectionErrorEventRouter_ =
        new mojo.internal.interfaceSupport.ConnectionErrorEventRouter;

    if (handle) {
      this.bindHandle(handle);
    }
  }

  /** @return {mojo.internal.interfaceSupport.Endpoint} */
  get endpoint() {
    return this.endpoint_;
  }

  /**
   * @return {!mojo.internal.interfaceSupport.PendingReceiver}
   */
  bindNewPipeAndPassReceiver() {
    let {handle0, handle1} = Mojo.createMessagePipe();
    this.bindHandle(handle0);
    return new this.requestType_(
        mojo.internal.interfaceSupport.createEndpoint(handle1));
  }

  /**
   * @param {!MojoHandle|!mojo.internal.interfaceSupport.Endpoint} handle
   * @export
   */
  bindHandle(handle) {
    console.assert(!this.endpoint_, 'already bound');
    handle = mojo.internal.interfaceSupport.createEndpoint(
        handle, /* setNamespaceBit */ true);
    this.endpoint_ = handle;
    this.endpoint_.start(this);
    this.pendingResponses_ = new Map;
  }

  /** @export */
  associateAndPassReceiver() {
    console.assert(!this.endpoint_, 'cannot associate when already bound');
    const {endpoint0, endpoint1} =
        mojo.internal.interfaceSupport.Endpoint.createAssociatedPair();
    this.bindHandle(endpoint0);
    return new this.requestType_(endpoint1);
  }

  /**
   * @return {?mojo.internal.interfaceSupport.Endpoint}
   * @export
   */
  unbind() {
    if (!this.endpoint_) {
      return null;
    }
    const endpoint = this.endpoint_;
    this.endpoint_ = null;
    endpoint.stop();
    return endpoint;
  }

  /** @export */
  close() {
    this.cleanupAndFlushPendingResponses_('Message pipe closed.');
    if (this.endpoint_) {
      this.endpoint_.close();
    }
    this.endpoint_ = null;
  }

  /**
   * @return {!mojo.internal.interfaceSupport.ConnectionErrorEventRouter}
   * @export
   */
  getConnectionErrorEventRouter() {
    return this.connectionErrorEventRouter_;
  }

  /**
   * @param {number} ordinal
   * @param {!mojo.internal.MojomType} paramStruct
   * @param {?mojo.internal.MojomType} maybeResponseStruct
   * @param {!Array} args
   * @return {!Promise}
   * @export
   */
  sendMessage(ordinal, paramStruct, maybeResponseStruct, args) {
    // The pipe has already been closed, so just drop the message.
    if (maybeResponseStruct && (!this.endpoint_ || !this.endpoint_.isStarted)) {
      return Promise.reject(new Error('The pipe has already been closed.'));
    }

    const value = {};
    paramStruct.$.structSpec.fields.forEach(
        (field, index) => value[field.name] = args[index]);
    const requestId = this.endpoint_.generateRequestId();
    this.endpoint_.send(
        ordinal, requestId,
        maybeResponseStruct ? mojo.internal.kMessageFlagExpectsResponse : 0,
        paramStruct, value);
    if (!maybeResponseStruct) {
      return Promise.resolve();
    }

    const responseStruct =
        /** @type {!mojo.internal.MojomType} */ (maybeResponseStruct);
    return new Promise((resolve, reject) => {
      this.pendingResponses_.set(
          requestId, {requestId, ordinal, responseStruct, resolve, reject});
    });
  }

  /**
   * @return {!Promise}
   * @export
   */
  flushForTesting() {
    return this.endpoint_.flushForTesting();
  }

  /** @override */
  onMessageReceived(endpoint, header, buffer, handles) {
    if (!(header.flags & mojo.internal.kMessageFlagIsResponse) ||
        header.flags & mojo.internal.kMessageFlagExpectsResponse) {
      return this.onError(endpoint, 'Received unexpected request message');
    }
    const pendingResponse = this.pendingResponses_.get(header.requestId);
    this.pendingResponses_.delete(header.requestId);
    if (!pendingResponse)
      return this.onError(endpoint, 'Received unexpected response message');
    const decoder = new mojo.internal.Decoder(
        new DataView(buffer, header.headerSize), handles, {endpoint});
    const responseValue = decoder.decodeStructInline(
        /** @type {!mojo.internal.StructSpec} */ (
            pendingResponse.responseStruct.$.structSpec));
    if (!responseValue)
      return this.onError(endpoint, 'Received malformed response message');
    if (header.ordinal !== pendingResponse.ordinal)
      return this.onError(endpoint, 'Received malformed response message');

    pendingResponse.resolve(responseValue);
  }

  /** @override */
  onError(endpoint, reason = undefined) {
    this.cleanupAndFlushPendingResponses_(reason);
    this.connectionErrorEventRouter_.dispatchErrorEvent();
  }

  /**
   * @param {string=} reason
   * @private
   */
  cleanupAndFlushPendingResponses_(reason = undefined) {
    if (this.endpoint_) {
      this.endpoint_.stop();
    }
    for (const id of this.pendingResponses_.keys()) {
      this.pendingResponses_.get(id).reject(new Error(reason));
    }
    this.pendingResponses_ = new Map;
  }
};

/**
 * Wrapper around mojo.internal.interfaceSupport.InterfaceRemoteBase that
 * exposes the subset of InterfaceRemoteBase's method that users are allowed
 * to use.
 * @template T
 * @export
 */
mojo.internal.interfaceSupport.InterfaceRemoteBaseWrapper = class {
  /**
   * @param {!mojo.internal.interfaceSupport.InterfaceRemoteBase<T>} remote
   * @public
   */
  constructor(remote) {
    /** @private {!mojo.internal.interfaceSupport.InterfaceRemoteBase<T>} */
    this.remote_ = remote;
  }

  /**
   * @return {!T}
   * @export
   */
  bindNewPipeAndPassReceiver() {
    return this.remote_.bindNewPipeAndPassReceiver();
  }

  /**
   * @return {!T}
   * @export
   */
  associateAndPassReceiver() {
    return this.remote_.associateAndPassReceiver();
  }

  /**
   * @return {boolean}
   * @export
   */
  isBound() {
    return this.remote_.endpoint_ !== null;
  }

  /** @export */
  close() {
    this.remote_.close();
  }

  /**
   * @return {!Promise}
   * @export
   */
  flushForTesting() {
    return this.remote_.flushForTesting();
  }
}

/**
 * Helper used by generated EventRouter types to dispatch incoming interface
 * messages as Event-like things.
 * @export
 */
mojo.internal.interfaceSupport.CallbackRouter = class {
  constructor() {
    /** @type {!Map<number, !Function>} */
    this.removeCallbacks = new Map;

    /** @private {number} */
    this.nextListenerId_ = 0;
  }

  /** @return {number} */
  getNextId() {
    return ++this.nextListenerId_;
  }

  /**
   * @param {number} id An ID returned by a prior call to addListener.
   * @return {boolean} True iff the identified listener was found and removed.
   * @export
   */
  removeListener(id) {
    this.removeCallbacks.get(id)();
    return this.removeCallbacks.delete(id);
  }
};

/**
 * Helper used by generated CallbackRouter types to dispatch incoming interface
 * messages to listeners.
 * @export
 */
mojo.internal.interfaceSupport.InterfaceCallbackReceiver = class {
  /**
   * @public
   * @param {!mojo.internal.interfaceSupport.CallbackRouter} callbackRouter
   */
  constructor(callbackRouter) {
    /** @private {!Map<number, !Function>} */
    this.listeners_ = new Map;

    /** @private {!mojo.internal.interfaceSupport.CallbackRouter} */
    this.callbackRouter_ = callbackRouter;
  }

  /**
   * @param {!Function} listener
   * @return {number} A unique ID for the added listener.
   * @export
   */
  addListener(listener) {
    const id = this.callbackRouter_.getNextId();
    this.listeners_.set(id, listener);
    this.callbackRouter_.removeCallbacks.set(id, () => {
      return this.listeners_.delete(id);
    });
    return id;
  }

  /**
   * @param {boolean} expectsResponse
   * @return {!Function}
   * @export
   */
  createReceiverHandler(expectsResponse) {
    if (expectsResponse)
      return this.dispatchWithResponse_.bind(this);
    return this.dispatch_.bind(this);
  }

  /**
   * @param {...*} varArgs
   * @private
   */
  dispatch_(varArgs) {
    const args = Array.from(arguments);
    this.listeners_.forEach(listener => listener.apply(null, args));
  }

  /**
   * @param {...*} varArgs
   * @return {?Object}
   * @private
   */
  dispatchWithResponse_(varArgs) {
    const args = Array.from(arguments);
    const returnValues = Array.from(this.listeners_.values())
                             .map(listener => listener.apply(null, args));

    let returnValue;
    for (const value of returnValues) {
      if (value === undefined)
        continue;
      if (returnValue !== undefined)
        throw new Error('Multiple listeners attempted to reply to a message');
      returnValue = value;
    }

    return returnValue;
  }
};

/**
 * Wraps message handlers attached to an InterfaceReceiver.
 *
 * @typedef {{
 *   paramStruct: !mojo.internal.MojomType,
 *   responseStruct: ?mojo.internal.MojomType,
 *   handler: !Function,
 * }}
 */
mojo.internal.interfaceSupport.MessageHandler;

/**
 * Generic helper that listens for incoming request messages on one or more
 * endpoints of the same interface type, dispatching them to registered
 * handlers. Handlers are registered against a specific ordinal message number.
 *
 * @template T
 * @implements {mojo.internal.interfaceSupport.EndpointClient}
 * @export
 */
mojo.internal.interfaceSupport.InterfaceReceiverHelperInternal = class {
  /**
   * @param {!function(new:T,
   *     (!MojoHandle|!mojo.internal.interfaceSupport.Endpoint)=)} remoteType
   * @public
   */
  constructor(remoteType) {
    /** @private {!Set<!mojo.internal.interfaceSupport.Endpoint>} endpoints */
    this.endpoints_ = new Set();

    /**
     * @private {!function(new:T,
     *     (!MojoHandle|!mojo.internal.interfaceSupport.Endpoint)=)}
     */
    this.remoteType_ = remoteType;

    /**
     * @private {!Map<number, !mojo.internal.interfaceSupport.MessageHandler>}
     */
    this.messageHandlers_ = new Map;

    /** @const {!mojo.internal.interfaceSupport.ConnectionErrorEventRouter} */
    this.connectionErrorEventRouter_ =
        new mojo.internal.interfaceSupport.ConnectionErrorEventRouter;
  }

  /**
   * @param {number} ordinal
   * @param {!mojo.internal.MojomType} paramStruct
   * @param {?mojo.internal.MojomType} responseStruct
   * @param {!Function} handler
   * @export
   */
  registerHandler(ordinal, paramStruct, responseStruct, handler) {
    this.messageHandlers_.set(ordinal, {paramStruct, responseStruct, handler});
  }

  /**
   * @param {!MojoHandle|!mojo.internal.interfaceSupport.Endpoint} handle
   * @export
   */
  bindHandle(handle) {
    handle = mojo.internal.interfaceSupport.createEndpoint(handle);
    this.endpoints_.add(handle);
    handle.start(this);
  }

  /**
   * @return {!T}
   * @export
   */
  bindNewPipeAndPassRemote() {
    let remote = new this.remoteType_();
    this.bindHandle(remote.$.bindNewPipeAndPassReceiver().handle);
    return remote;
  }

  /**
   * @return {!T}
   * @export
   */
  associateAndPassRemote() {
    const {endpoint0, endpoint1} =
        mojo.internal.interfaceSupport.Endpoint.createAssociatedPair();
    this.bindHandle(endpoint0);
    return new this.remoteType_(endpoint1);
  }

  /** @export */
  closeBindings() {
    for (const endpoint of this.endpoints_) {
      endpoint.close();
    }
    this.endpoints_.clear();
  }

  /**
   * @return {!mojo.internal.interfaceSupport.ConnectionErrorEventRouter}
   * @export
   */
  getConnectionErrorEventRouter() {
    return this.connectionErrorEventRouter_;
  }

  /**
   * @return {!Promise}
   * @export
   */
  async flush() {
    for (let endpoint of this.endpoints_) {
      await endpoint.flushForTesting();
    }
  }

  /** @override */
  onMessageReceived(endpoint, header, buffer, handles) {
    if (header.flags & mojo.internal.kMessageFlagIsResponse)
      throw new Error('Received unexpected response on interface receiver');
    const handler = this.messageHandlers_.get(header.ordinal);
    if (!handler)
      throw new Error('Received unknown message');
    const decoder = new mojo.internal.Decoder(
        new DataView(buffer, header.headerSize), handles, {endpoint});
    const request = decoder.decodeStructInline(
        /** @type {!mojo.internal.StructSpec} */ (
            handler.paramStruct.$.structSpec));
    if (!request)
      throw new Error('Received malformed message');

    let result = handler.handler.apply(
        null,
        handler.paramStruct.$.structSpec.fields.map(
            field => request[field.name]));

    // If the message expects a response, the handler must return either a
    // well-formed response object, or a Promise that will eventually yield one.
    if (handler.responseStruct) {
      if (result === undefined) {
        this.onError(endpoint);
        throw new Error(
            'Message expects a reply but its handler did not provide one.');
      }

      if (typeof result != 'object' || result.constructor.name != 'Promise') {
        result = Promise.resolve(result);
      }

      result
          .then(value => {
            endpoint.send(
                header.ordinal, header.requestId,
                mojo.internal.kMessageFlagIsResponse,
                /** @type {!mojo.internal.MojomType} */
                (handler.responseStruct), value);
          })
          .catch(() => {
            // If the handler rejects, that means it didn't like the request's
            // contents for whatever reason. We close the binding to prevent
            // further messages from being received from that client.
            this.onError(endpoint);
          });
    }
  }

  /** @override */
  onError(endpoint, reason = undefined) {
    this.endpoints_.delete(endpoint);
    endpoint.close();
    this.connectionErrorEventRouter_.dispatchErrorEvent();
  }
};

/**
 * Generic helper used to perform operations related to the interface pipe e.g.
 * bind the pipe, close it, flush it for testing, etc. Wraps
 * mojo.internal.interfaceSupport.InterfaceReceiverHelperInternal and exposes a
 * subset of methods that meant to be used by users of a receiver class.
 *
 * @template T
 * @export
 */
mojo.internal.interfaceSupport.InterfaceReceiverHelper = class {
  /**
   * @param {!mojo.internal.interfaceSupport.InterfaceReceiverHelperInternal<T>}
   *     helper_internal
   * @public
   */
  constructor(helper_internal) {
    /**
     * @private {!mojo.internal.interfaceSupport.InterfaceReceiverHelperInternal<T>}
     */
    this.helper_internal_ = helper_internal;
  }

  /**
   * Binds a new handle to this object. Messages which arrive on the handle will
   * be read and dispatched to this object.
   *
   * @param {!MojoHandle|!mojo.internal.interfaceSupport.Endpoint} handle
   * @export
   */
  bindHandle(handle) {
    this.helper_internal_.bindHandle(handle);
  }

  /**
   * @return {!T}
   * @export
   */
  bindNewPipeAndPassRemote() {
    return this.helper_internal_.bindNewPipeAndPassRemote();
  }

  /**
   * @return {!T}
   * @export
   */
  associateAndPassRemote() {
    return this.helper_internal_.associateAndPassRemote();
  }

  /** @export */
  close() {
    this.helper_internal_.closeBindings();
  }

  /**
   * @return {!Promise}
   * @export
   */
  flush() {
    return this.helper_internal_.flush();
  }
}

/**
 * Watches a MojoHandle for readability or peer closure, forwarding either event
 * to one of two callbacks on the reader. Used by both InterfaceRemoteBase and
 * InterfaceReceiverHelperInternal to watch for incoming messages.
 */
mojo.internal.interfaceSupport.HandleReader = class {
  /**
   * @param {!MojoHandle} handle
   * @private
   */
  constructor(handle) {
    /** @private {!MojoHandle} */
    this.handle_ = handle;

    /** @public {?function(!ArrayBuffer, !Array<MojoHandle>)} */
    this.onRead = null;

    /** @public {!Function} */
    this.onError = () => {};

    /** @public {?MojoWatcher} */
    this.watcher_ = null;
  }

  isStopped() {
    return this.watcher_ === null;
  }

  start() {
    this.watcher_ = this.handle_.watch({readable: true}, this.read_.bind(this));
  }

  stop() {
    if (!this.watcher_) {
      return;
    }
    this.watcher_.cancel();
    this.watcher_ = null;
  }

  stopAndCloseHandle() {
    if (this.watcher_) {
      this.stop();
    }
    this.handle_.close();
  }

  /** @private */
  read_(result) {
    for (;;) {
      if (!this.watcher_)
        return;

      const read = this.handle_.readMessage();

      // No messages available.
      if (read.result == Mojo.RESULT_SHOULD_WAIT)
        return;

      // Remote endpoint has been closed *and* no messages available.
      if (read.result == Mojo.RESULT_FAILED_PRECONDITION) {
        this.onError();
        return;
      }

      // Something terrible happened.
      if (read.result != Mojo.RESULT_OK)
        throw new Error('Unexpected error on HandleReader: ' + read.result);

      this.onRead(read.buffer, read.handles);
    }
  }
};
export {mojo};

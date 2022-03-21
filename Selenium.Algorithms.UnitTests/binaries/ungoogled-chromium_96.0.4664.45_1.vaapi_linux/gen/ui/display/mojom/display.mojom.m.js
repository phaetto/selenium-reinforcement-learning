// ui/display/mojom/display.mojom.m.js is auto generated by mojom_bindings_generator.py, do not edit

// Copyright 2020 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import {mojo} from '../../../mojo/public/js/bindings.js';

import {
  Rect as gfx_mojom_Rect,
  RectSpec as gfx_mojom_RectSpec,
  Size as gfx_mojom_Size,
  SizeSpec as gfx_mojom_SizeSpec
} from '../../gfx/geometry/mojom/geometry.mojom.m.js';

import {
  DisplayColorSpaces as gfx_mojom_DisplayColorSpaces,
  DisplayColorSpacesSpec as gfx_mojom_DisplayColorSpacesSpec
} from '../../gfx/mojom/display_color_spaces.mojom.m.js';


/**
 * @const { {$: !mojo.internal.MojomType} }
 */
export const RotationSpec = { $: mojo.internal.Enum() };

/**
 * @enum {number}
 */
export const Rotation = {
  
  VALUE_0: 0,
  VALUE_90: 1,
  VALUE_180: 2,
  VALUE_270: 3,
  MIN_VALUE: 0,
  MAX_VALUE: 3,
};

/**
 * @const { {$: !mojo.internal.MojomType} }
 */
export const TouchSupportSpec = { $: mojo.internal.Enum() };

/**
 * @enum {number}
 */
export const TouchSupport = {
  
  UNKNOWN: 0,
  AVAILABLE: 1,
  UNAVAILABLE: 2,
  MIN_VALUE: 0,
  MAX_VALUE: 2,
};

/**
 * @const { {$: !mojo.internal.MojomType} }
 */
export const AccelerometerSupportSpec = { $: mojo.internal.Enum() };

/**
 * @enum {number}
 */
export const AccelerometerSupport = {
  
  UNKNOWN: 0,
  AVAILABLE: 1,
  UNAVAILABLE: 2,
  MIN_VALUE: 0,
  MAX_VALUE: 2,
};


/**
 * @const { {$:!mojo.internal.MojomType}}
 */
export const DisplaySpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };




mojo.internal.Struct(
    DisplaySpec.$,
    'Display',
    [
      mojo.internal.StructField(
        'id', 0,
        0,
        mojo.internal.Int64,
        BigInt(0),
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'bounds', 8,
        0,
        gfx_mojom_RectSpec.$,
        null,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'sizeInPixels', 16,
        0,
        gfx_mojom_SizeSpec.$,
        null,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'workArea', 24,
        0,
        gfx_mojom_RectSpec.$,
        null,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'deviceScaleFactor', 32,
        0,
        mojo.internal.Float,
        0,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'rotation', 36,
        0,
        RotationSpec.$,
        0,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'touchSupport', 40,
        0,
        TouchSupportSpec.$,
        0,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'accelerometerSupport', 44,
        0,
        AccelerometerSupportSpec.$,
        0,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'maximumCursorSize', 48,
        0,
        gfx_mojom_SizeSpec.$,
        null,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'colorSpaces', 56,
        0,
        gfx_mojom_DisplayColorSpacesSpec.$,
        null,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'colorDepth', 64,
        0,
        mojo.internal.Int32,
        0,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'depthPerComponent', 68,
        0,
        mojo.internal.Int32,
        0,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'isMonochrome', 72,
        0,
        mojo.internal.Bool,
        false,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'displayFrequency', 76,
        0,
        mojo.internal.Int32,
        0,
        false /* nullable */,
        0),
    ],
    [[0, 88],]);



/**
 * @record
 */
export class Display {
  constructor() {
    /** @type { !bigint } */
    this.id;
    /** @type { !gfx_mojom_Rect } */
    this.bounds;
    /** @type { !gfx_mojom_Size } */
    this.sizeInPixels;
    /** @type { !gfx_mojom_Rect } */
    this.workArea;
    /** @type { !number } */
    this.deviceScaleFactor;
    /** @type { !Rotation } */
    this.rotation;
    /** @type { !TouchSupport } */
    this.touchSupport;
    /** @type { !AccelerometerSupport } */
    this.accelerometerSupport;
    /** @type { !gfx_mojom_Size } */
    this.maximumCursorSize;
    /** @type { !gfx_mojom_DisplayColorSpaces } */
    this.colorSpaces;
    /** @type { !number } */
    this.colorDepth;
    /** @type { !number } */
    this.depthPerComponent;
    /** @type { !boolean } */
    this.isMonochrome;
    /** @type { !number } */
    this.displayFrequency;
  }
}


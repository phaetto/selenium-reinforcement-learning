// ui/gfx/mojom/delegated_ink_point.mojom.m.js is auto generated by mojom_bindings_generator.py, do not edit

// Copyright 2020 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import {mojo} from '../../../mojo/public/js/bindings.js';

import {
  TimeTicks as mojoBase_mojom_TimeTicks,
  TimeTicksSpec as mojoBase_mojom_TimeTicksSpec
} from '../../../mojo/public/mojom/base/time.mojom.m.js';

import {
  PointF as gfx_mojom_PointF,
  PointFSpec as gfx_mojom_PointFSpec
} from '../geometry/mojom/geometry.mojom.m.js';



/**
 * @const { {$:!mojo.internal.MojomType}}
 */
export const DelegatedInkPointSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };




mojo.internal.Struct(
    DelegatedInkPointSpec.$,
    'DelegatedInkPoint',
    [
      mojo.internal.StructField(
        'point', 0,
        0,
        gfx_mojom_PointFSpec.$,
        null,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'timestamp', 8,
        0,
        mojoBase_mojom_TimeTicksSpec.$,
        null,
        false /* nullable */,
        0),
      mojo.internal.StructField(
        'pointerId', 16,
        0,
        mojo.internal.Int32,
        0,
        false /* nullable */,
        0),
    ],
    [[0, 32],]);



/**
 * @record
 */
export class DelegatedInkPoint {
  constructor() {
    /** @type { !gfx_mojom_PointF } */
    this.point;
    /** @type { !mojoBase_mojom_TimeTicks } */
    this.timestamp;
    /** @type { !number } */
    this.pointerId;
  }
}


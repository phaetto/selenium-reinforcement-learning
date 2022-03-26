// ui/gfx/mojom/gpu_fence_handle.mojom.m.js is auto generated by mojom_bindings_generator.py, do not edit

// Copyright 2020 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import {mojo} from '../../../mojo/public/js/bindings.js';



/**
 * @const { {$:!mojo.internal.MojomType}}
 */
export const GpuFenceHandleSpec =
    { $: /** @type {!mojo.internal.MojomType} */ ({}) };




mojo.internal.Struct(
    GpuFenceHandleSpec.$,
    'GpuFenceHandle',
    [
      mojo.internal.StructField(
        'nativeFd', 0,
        0,
        mojo.internal.Handle,
        null,
        false /* nullable */,
        0),
    ],
    [[0, 16],]);



/**
 * @record
 */
export class GpuFenceHandle {
  constructor() {
    /** @type { !MojoHandle } */
    this.nativeFd;
  }
}

// ui/gfx/mojom/font_render_params.mojom.m.js is auto generated by mojom_bindings_generator.py, do not edit

// Copyright 2020 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

import {mojo} from '../../../mojo/public/js/bindings.js';


/**
 * @const { {$: !mojo.internal.MojomType} }
 */
export const HintingSpec = { $: mojo.internal.Enum() };

/**
 * @enum {number}
 */
export const Hinting = {
  
  kNone: 0,
  kSlight: 1,
  kMedium: 2,
  kFull: 3,
  MIN_VALUE: 0,
  MAX_VALUE: 3,
};

/**
 * @const { {$: !mojo.internal.MojomType} }
 */
export const SubpixelRenderingSpec = { $: mojo.internal.Enum() };

/**
 * @enum {number}
 */
export const SubpixelRendering = {
  
  kNone: 0,
  kRGB: 1,
  kBGR: 2,
  kVRGB: 3,
  kVBGR: 4,
  MIN_VALUE: 0,
  MAX_VALUE: 4,
};




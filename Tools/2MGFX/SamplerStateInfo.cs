﻿using Microsoft.Xna.Framework.Graphics;

namespace TwoMGFX
{
    public class SamplerStateInfo
    {
        private SamplerState _state;
        
        private bool _dirty;

        private TextureFilterType _minFilter;
        private TextureFilterType _magFilter;
        private TextureFilterType _mipFilter;

        private TextureAddressMode _addressU;
        private TextureAddressMode _addressV;
        private TextureAddressMode _addressW;

        private int _maxAnisotropy;
        private int _maxMipLevel;
        private float _mipMapLevelOfDetailBias;

        public string Name { get; set; }

        public string TextureName { get; set; }

        public TextureFilterType MinFilter
        {
            set
            {
                _minFilter = value;
                _dirty = true;
            }
        }

        public TextureFilterType MagFilter
        {
            set
            {
                _magFilter = value;
                _dirty = true;
            }
        }

        public TextureFilterType MipFilter
        {
            set
            {
                _mipFilter = value;
                _dirty = true;
            }
        }

        public TextureAddressMode AddressU
        {
            set
            {
                _addressU = value;
                _dirty = true;
            }
        }

        public TextureAddressMode AddressV
        {
            set
            {
                _addressV = value;
                _dirty = true;
            }
        }

        public TextureAddressMode AddressW
        {
            set
            {
                _addressW = value;
                _dirty = true;
            }
        }

        public int MaxAnisotropy
        {
            set
            {
                _maxAnisotropy = value;
                _dirty = true;
            }
        }

        public int MaxMipLevel
        {
            set
            {
                _maxMipLevel = value;
                _dirty = true;
            }
        }

        public float MipMapLevelOfDetailBias
        {
            set
            {
                _mipMapLevelOfDetailBias = value;
                _dirty = true;
            }
        }

        private void UpdateSamplerState()
        {
            // Get the existing state or create it.
            if (_state == null)
                _state = new SamplerState();

            _state.AddressU = _addressU;
            _state.AddressV = _addressV;
            _state.AddressW = _addressW;

            _state.MaxAnisotropy = _maxAnisotropy;
            _state.MaxMipLevel = _maxMipLevel;
            _state.MipMapLevelOfDetailBias = _mipMapLevelOfDetailBias;

            // Figure out what kind of filter to set based on each
            // individual min, mag, and mip filter settings.
            if (_minFilter == TextureFilterType.Anisotropic)
                _state.Filter = TextureFilter.Anisotropic;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.Linear;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.LinearMipPoint;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.MinLinearMagPointMipLinear;
            else if (_minFilter == TextureFilterType.Linear && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.MinLinearMagPointMipPoint;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.MinPointMagLinearMipLinear;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Linear && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.MinPointMagLinearMipPoint;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Point)
                _state.Filter = TextureFilter.Point;
            else if (_minFilter == TextureFilterType.Point && _magFilter == TextureFilterType.Point && _mipFilter == TextureFilterType.Linear)
                _state.Filter = TextureFilter.PointMipLinear;

            _dirty = false;
        }

        public void Parse(string name, string value)
        {
            switch (name.ToLower())
            {
                case "texture":
                    TextureName = value;
                    break;
                case "minfilter":
                    MinFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "magfilter":
                    MagFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "mipfilter":
                    MipFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "filter":
                    MinFilter = MagFilter = MipFilter = ParseTreeTools.ParseTextureFilterType(value);
                    break;
                case "addressu":
                    AddressU = ParseTreeTools.ParseAddressMode(value);
                    break;
                case "addressv":
                    AddressV = ParseTreeTools.ParseAddressMode(value);
                    break;
                case "addressw":
                    AddressW = ParseTreeTools.ParseAddressMode(value);
                    break;
                case "maxanisotropy":
                    MaxAnisotropy = int.Parse(value);
                    break;
                case "maxlod":
                    MaxMipLevel = int.Parse(value);
                    break;
                case "miplodbias":
                    MipMapLevelOfDetailBias = float.Parse(value);
                    break;
            }            
        }

        public SamplerState State
        {
            get
            {
                if (_dirty)
                    UpdateSamplerState();

                return _state;
            }
        }
    }
}
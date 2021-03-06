﻿using NTMiner.Core;
using System;

namespace NTMiner.Vms {
    public class SpeedViewModel : ViewModelBase, ISpeed {
        private long _speed;
        private DateTime _speedOn;

        public SpeedViewModel(ISpeed speed) {
            this.Value = speed.Value;
        }

        public void Update(ISpeed data) {
            this.Value = data.Value;
            this.SpeedOn = data.SpeedOn;
        }

        public long Value {
            get => _speed;
            set {
                _speed = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(SpeedText));
            }
        }

        public string SpeedText {
            get {
                return Value.ToUnitSpeedText();
            }
        }

        public DateTime SpeedOn {
            get {
                return _speedOn;
            }
            set {
                _speedOn = value;
                OnPropertyChanged(nameof(SpeedOn));
            }
        }
    }
}

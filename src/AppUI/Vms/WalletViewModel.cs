﻿using NTMiner.Core;
using NTMiner.ServiceContracts.DataObjects;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class WalletViewModel : ViewModelBase, IWallet {
        public static readonly WalletViewModel PleaseSelect = new WalletViewModel(Guid.Empty) {
            _name = "请选择"
        };

        private Guid _id;
        private string _name;
        private Guid _coinId;
        private string _address;
        private int _sortNumber;
        private bool _isCurrentWallet;
        private bool _isCurrentDualCoinWallet;

        public Guid GetId() {
            return this.Id;
        }

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand SortUp { get; private set; }
        public ICommand SortDown { get; private set; }
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public WalletViewModel(IWallet data) : this(data.GetId()) {
            _name = data.Name;
            _coinId = data.CoinId;
            _address = data.Address;
            _sortNumber = data.SortNumber;
        }

        public WalletViewModel(Guid id) {
            _id = id;
            this.Save = new DelegateCommand(() => {
                if (!this.IsTestWallet) {
                    if (NTMinerRoot.Current.WalletSet.Contains(this.Id)) {
                        Global.Execute(new UpdateWalletCommand(this));
                    }
                    else {
                        Global.Execute(new AddWalletCommand(this));
                    }
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand(() => {
                WalletEdit.ShowEditWindow(this);
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                if (this.IsTestWallet) {
                    return;
                }
                DialogWindow.ShowDialog(message: $"您确定删除{this.Name}钱包吗？", title: "确认", onYes: () => {
                    Global.Execute(new RemoveWalletCommand(this.Id));
                }, icon: "Icon_Confirm");
            });
            this.SortUp = new DelegateCommand(() => {
                WalletViewModel upOne = WalletViewModels.Current.WalletList.OrderByDescending(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber < this.SortNumber);
                if (upOne != null) {
                    int sortNumber = upOne.SortNumber;
                    upOne.SortNumber = this.SortNumber;
                    Global.Execute(new UpdateWalletCommand(upOne));
                    this.SortNumber = sortNumber;
                    Global.Execute(new UpdateWalletCommand(this));
                    CoinViewModel coinVm;
                    if (CoinViewModels.Current.TryGetCoinVm(this.CoinId, out coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.Wallets));
                    }
                }
            });
            this.SortDown = new DelegateCommand(() => {
                WalletViewModel nextOne = WalletViewModels.Current.WalletList.OrderBy(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber > this.SortNumber);
                if (nextOne != null) {
                    int sortNumber = nextOne.SortNumber;
                    nextOne.SortNumber = this.SortNumber;
                    Global.Execute(new UpdateWalletCommand(nextOne));
                    this.SortNumber = sortNumber;
                    Global.Execute(new UpdateWalletCommand(this));
                    CoinViewModel coinVm;
                    if (CoinViewModels.Current.TryGetCoinVm(this.CoinId, out coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.Wallets));
                    }
                }
            });
        }

        public Guid Id {
            get => _id;
            private set {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                    if (string.IsNullOrEmpty(value)) {
                        throw new ValidationException("别名是必须的");
                    }
                }
            }
        }

        public int SortNumber {
            get => _sortNumber;
            set {
                _sortNumber = value;
                OnPropertyChanged(nameof(SortNumber));
            }
        }

        public Guid CoinId {
            get => _coinId;
            set {
                _coinId = value;
                OnPropertyChanged(nameof(CoinId));
                OnPropertyChanged(nameof(CoinCode));
            }
        }

        public string CoinCode {
            get {
                ICoin coin;
                if (NTMinerRoot.Current.CoinSet.TryGetCoin(this.CoinId, out coin)) {
                    return coin.Code;
                }
                return string.Empty;
            }
        }

        public ICoin Coin {
            get {
                ICoin coin;
                if (NTMinerRoot.Current.CoinSet.TryGetCoin(this.CoinId, out coin)) {
                    return coin;
                }
                return CoinViewModel.Empty;
            }
        }

        public bool IsTestWallet {
            get {
                return Coin.GetId() == this.Id;
            }
        }

        public bool IsNotTestWallet {
            get {
                return !IsTestWallet;
            }
        }

        public string Address {
            get => _address;
            set {
                if (_address != value) {
                    _address = value ?? string.Empty;
                    OnPropertyChanged(nameof(Address));
                    if (!string.IsNullOrEmpty(Coin.WalletRegexPattern)) {
                        Regex regex = new Regex(Coin.WalletRegexPattern);
                        if (!regex.IsMatch(value ?? string.Empty)) {
                            throw new ValidationException("钱包地址格式不正确。");
                        }
                    }
                }
            }
        }

        public bool IsAddressEditable {
            get {
                if (IsTestWallet) {
                    return false;
                }
                if (NTMinerRoot.Current.WalletSet.Contains(this.Id)) {
                    return false;
                }
                return true;
            }
        }

        public bool IsCurrentWallet {
            get => _isCurrentWallet;
            set {
                _isCurrentWallet = value;
                OnPropertyChanged(nameof(IsCurrentWallet));
            }
        }

        public bool IsCurrentDualCoinWallet {
            get => _isCurrentDualCoinWallet;
            set {
                _isCurrentDualCoinWallet = value;
                OnPropertyChanged(nameof(IsCurrentDualCoinWallet));
            }
        }
    }
}

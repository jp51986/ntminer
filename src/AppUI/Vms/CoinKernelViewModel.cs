﻿using NTMiner.Core;
using NTMiner.Core.Gpus;
using NTMiner.Core.Kernels;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class CoinKernelViewModel : ViewModelBase, ICoinKernel {
        private Guid _id;
        private Guid _coinId;
        private Guid _kernelId;
        private int _sortNumber;
        private Guid _dualCoinGroupId;
        private string _args;
        private string _description;
        private SupportedGpu _supportedGpu;

        public Guid GetId() {
            return this.Id;
        }

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand SortUp { get; private set; }
        public ICommand SortDown { get; private set; }
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public CoinKernelViewModel(ICoinKernel data) : this(data.GetId()) {
            _coinId = data.CoinId;
            _kernelId = data.KernelId;
            _sortNumber = data.SortNumber;
            _dualCoinGroupId = data.DualCoinGroupId;
            _args = data.Args;
            _description = data.Description;
            _supportedGpu = data.SupportedGpu;
        }

        public CoinKernelViewModel(Guid id) {
            _id = id;
            this.Save = new DelegateCommand(() => {
                if (NTMinerRoot.Current.CoinKernelSet.Contains(this.Id)) {
                    Global.Execute(new UpdateCoinKernelCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand(() => {
                CoinKernelEdit.ShowEditWindow(this);
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                DialogWindow.ShowDialog(message: $"您确定删除{Kernel.Code}币种内核吗？", title: "确认", onYes: () => {
                    Global.Execute(new RemoveCoinKernelCommand(this.Id));
                    Kernel.OnPropertyChanged(nameof(Kernel.SupportedCoins));
                }, icon: "Icon_Confirm");
            });
            this.SortUp = new DelegateCommand(() => {
                CoinKernelViewModel upOne = CoinKernelViewModels.Current.AllCoinKernels.OrderByDescending(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber < this.SortNumber);
                if (upOne != null) {
                    int sortNumber = upOne.SortNumber;
                    upOne.SortNumber = this.SortNumber;
                    Global.Execute(new UpdateCoinKernelCommand(upOne));
                    this.SortNumber = sortNumber;
                    Global.Execute(new UpdateCoinKernelCommand(this));
                    CoinViewModel coinVm;
                    if (CoinViewModels.Current.TryGetCoinVm(this.CoinId, out coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.CoinKernels));
                    }
                    this.Kernel.OnPropertyChanged(nameof(this.Kernel.CoinKernels));
                    CoinViewModels.Current.OnPropertyChanged(nameof(CoinViewModels.MainCoins));
                }
            });
            this.SortDown = new DelegateCommand(() => {
                CoinKernelViewModel nextOne = CoinKernelViewModels.Current.AllCoinKernels.OrderBy(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber > this.SortNumber);
                if (nextOne != null) {
                    int sortNumber = nextOne.SortNumber;
                    nextOne.SortNumber = this.SortNumber;
                    Global.Execute(new UpdateCoinKernelCommand(nextOne));
                    this.SortNumber = sortNumber;
                    Global.Execute(new UpdateCoinKernelCommand(this));
                    CoinViewModel coinVm;
                    if (CoinViewModels.Current.TryGetCoinVm(this.CoinId, out coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.CoinKernels));
                    }
                    this.Kernel.OnPropertyChanged(nameof(this.Kernel.CoinKernels));
                    CoinViewModels.Current.OnPropertyChanged(nameof(CoinViewModels.MainCoins));
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

        public Guid CoinId {
            get {
                return _coinId;
            }
            set {
                _coinId = value;
                OnPropertyChanged(nameof(CoinId));
                OnPropertyChanged(nameof(CoinCode));
            }
        }

        public string CoinCode {
            get {
                return CoinVm.Code;
            }
        }

        private CoinViewModel _coinVm;
        public CoinViewModel CoinVm {
            get {
                if (_coinVm == null || this.CoinId != _coinVm.Id) {
                    CoinViewModels.Current.TryGetCoinVm(this.CoinId, out _coinVm);
                    if (_coinVm == null) {
                        _coinVm = CoinViewModel.Empty;
                    }
                }
                return _coinVm;
            }
        }

        public Guid KernelId {
            get => _kernelId;
            set {
                _kernelId = value;
                OnPropertyChanged(nameof(KernelId));
            }
        }

        public string DisplayName {
            get {
                return $"{Kernel.Code}{Kernel.Version}";
            }
        }

        public KernelViewModel Kernel {
            get {
                KernelViewModel kernel;
                if (KernelViewModels.Current.TryGetKernelVm(this.KernelId, out kernel)) {
                    return kernel;
                }
                return KernelViewModel.Empty;
            }
        }

        public int SortNumber {
            get => _sortNumber;
            set {
                _sortNumber = value;
                OnPropertyChanged(nameof(SortNumber));
            }
        }

        public Guid DualCoinGroupId {
            get => _dualCoinGroupId;
            set {
                _dualCoinGroupId = value;
                OnPropertyChanged(nameof(DualCoinGroupId));
            }
        }

        private GroupViewModel _selectedDualCoinGroup;
        public GroupViewModel SelectedDualCoinGroup {
            get {
                if (this.DualCoinGroupId == Guid.Empty) {
                    return GroupViewModel.PleaseSelect;
                }
                if (_selectedDualCoinGroup == null || _selectedDualCoinGroup.Id != this.DualCoinGroupId) {
                    GroupViewModels.Current.TryGetGroupVm(DualCoinGroupId, out _selectedDualCoinGroup);
                    if (_selectedDualCoinGroup == null) {
                        _selectedDualCoinGroup = GroupViewModel.PleaseSelect;
                    }
                }
                return _selectedDualCoinGroup;
            }
            set {
                if (DualCoinGroupId != value.Id) {
                    DualCoinGroupId = value.Id;
                    OnPropertyChanged(nameof(DualCoinGroup));
                    OnPropertyChanged(nameof(SelectedDualCoinGroup));
                    OnPropertyChanged(nameof(IsSupportDualMine));
                }
            }
        }

        public GroupViewModel DualCoinGroup {
            get {
                if (!this.Kernel.IsSupportDualMine) {
                    return GroupViewModel.PleaseSelect;
                }
                if (this.DualCoinGroupId == Guid.Empty) {
                    return this.Kernel.DualCoinGroup;
                }
                return SelectedDualCoinGroup;
            }
        }

        public GroupViewModels GroupVms {
            get {
                return GroupViewModels.Current;
            }
        }

        public string Args {
            get { return _args; }
            set {
                _args = value;
                OnPropertyChanged(nameof(Args));
            }
        }

        public string Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public bool IsSupportDualMine {
            get {
                if (!this.Kernel.IsSupportDualMine) {
                    return false;
                }
                if (this.DualCoinGroupId != Guid.Empty) {
                    return true;
                }
                return this.Kernel.DualCoinGroupId != Guid.Empty;
            }
        }

        public SupportedGpu SupportedGpu {
            get => _supportedGpu;
            set {
                _supportedGpu = value;
                OnPropertyChanged(nameof(SupportedGpu));
                OnPropertyChanged(nameof(IsNvidiaIconVisible));
                OnPropertyChanged(nameof(IsAMDIconVisible));
                OnPropertyChanged(nameof(IsSupported));
            }
        }

        public bool IsSupported {
            get {
                if (this.SupportedGpu == SupportedGpu.Both) {
                    return true;
                }
                if (this.SupportedGpu == SupportedGpu.NVIDIA && NTMinerRoot.Current.GpuSet.GpuType == GpuType.NVIDIA) {
                    return true;
                }
                if (this.SupportedGpu == SupportedGpu.AMD && NTMinerRoot.Current.GpuSet.GpuType == GpuType.AMD) {
                    return true;
                }
                return false;
            }
        }

        public Visibility IsNvidiaIconVisible {
            get {
                if (SupportedGpu == SupportedGpu.NVIDIA || SupportedGpu == SupportedGpu.Both) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility IsAMDIconVisible {
            get {
                if (SupportedGpu == SupportedGpu.AMD || SupportedGpu == SupportedGpu.Both) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public IEnumerable<EnumItem<SupportedGpu>> SupportedGpuEnumItems {
            get {
                return SupportedGpu.AMD.GetEnumItems();
            }
        }

        public EnumItem<SupportedGpu> SupportedGpuEnumItem {
            get {
                return SupportedGpuEnumItems.FirstOrDefault(a => a.Value == SupportedGpu);
            }
            set {
                SupportedGpu = value.Value;
                OnPropertyChanged(nameof(SupportedGpuEnumItem));
            }
        }

        public string SupportedGpuDescription {
            get {
                return SupportedGpu.GetDescription();
            }
        }

        public CoinKernelProfileViewModel CoinKernelProfile {
            get {
                return CoinProfileViewModels.Current.GetOrCreateCoinKernelProfileVm(this.Id);
            }
        }
    }
}

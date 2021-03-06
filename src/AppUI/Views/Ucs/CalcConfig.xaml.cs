﻿using NTMiner.Vms;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class CalcConfig : UserControl {
        public static void ShowWindow() {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                IconName = "Icon_Calc",
                Width = 560,
                Height = 450,
                CloseVisible = System.Windows.Visibility.Visible
            }, ucFactory: (window) => {
                var uc = new CalcConfig();
                CalcConfigViewModels vm = (CalcConfigViewModels)uc.DataContext;
                vm.CloseWindow = () => window.Close();
                uc.ItemsControl.MouseDown += (object sender, System.Windows.Input.MouseButtonEventArgs e)=> {
                    if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                        window.DragMove();
                    }
                };
                return uc;
            }, fixedSize: false);
        }

        public CalcConfigViewModels Vm {
            get {
                return (CalcConfigViewModels)this.DataContext;
            }
        }

        private CalcConfig() {
            InitializeComponent();
            ResourceDictionarySet.Instance.FillResourceDic(this, this.Resources);
        }
    }
}

﻿using NTMiner.Language;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NTMiner {
    public class ResourceDictionarySet {
        public static readonly ResourceDictionarySet Instance = new ResourceDictionarySet();

        private readonly Dictionary<string, ResourceDictionary> _dicByViewId = new Dictionary<string, ResourceDictionary>();

        private ResourceDictionarySet() {
            Global.Access<LangViewItemUpdatedEvent>(
                Guid.Parse("E537F225-11C6-4B0D-B2B3-BC111E4EFFEA"),
                "更新了语言视图项后刷新WPF动态资源集",
                LogEnum.None,
                action: message => {
                    ResourceDictionary resourceDictionary;
                    if (TryGetResourceDic(message.Source.ViewId, out resourceDictionary)) {
                        if (resourceDictionary.Contains(message.Source.Key) && Global.Lang.GetId() == message.Source.LangId) {
                            resourceDictionary[message.Source.Key] = message.Source.Value;
                        }
                    }
                });
            Global.Access<GlobalLangChangedEvent>(
                Guid.Parse("732B9E09-1F97-4A1D-80E4-094DFD2CCC9D"),
                "切换语言后刷新视图语言资源",
                LogEnum.None,
                action: message => {
                    foreach (var kv in _dicByViewId) {
                        FillResourceDic(kv.Key, kv.Value);
                    }
                });
        }

        public bool TryGetResourceDic(string viewId, out ResourceDictionary resourceDictionary) {
            resourceDictionary = null;
            if (!_dicByViewId.ContainsKey(viewId)) {
                return false;
            }
            return _dicByViewId.TryGetValue(viewId, out resourceDictionary);
        }

        private void FillResourceDic(string viewId, ResourceDictionary resourceDictionary) {
            if (!_dicByViewId.ContainsKey(viewId)) {
                _dicByViewId.Add(viewId, resourceDictionary);
            }
            IList<ILangViewItem> langItems = LangViewItemSet.Instance.GetLangItems(Global.Lang.GetId(), viewId);
            Type stringType = typeof(string);
            foreach (var item in langItems) {
                if (resourceDictionary.Contains(item.Key) && resourceDictionary[item.Key].GetType() == stringType) {
                    resourceDictionary[item.Key] = item.Value;
                }
            }
        }

        public void FillResourceDic(Control view, ResourceDictionary resourceDictionary) {
            string viewId = view.GetType().Name;
            if (_dicByViewId.ContainsKey(viewId)) {
                view.Resources = _dicByViewId[viewId];
            }
            FillResourceDic(viewId, resourceDictionary);
        }
    }
}

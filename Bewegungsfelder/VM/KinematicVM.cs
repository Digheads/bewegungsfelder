﻿/*
Part of Bewegungsfelder

MIT-License
(C) 2016 Ivo Herzig

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Bewegungsfelder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.VM
{
    public class KinematicVM
    {
        private BoneVM selectedItem;

        /// <summary>
        /// contains all Bone-BoneVM pairs
        /// </summary>
        public Dictionary<Bone, BoneVM> BoneVMMap { get; } = new Dictionary<Bone, BoneVM>();

        /// <summary>
        /// the underlying model for this view model
        /// </summary>
        public KinematicStructure Model { get; }

        /// <summary>
        /// roots collection is always just a single entry. Used to be able to bind to collection views
        /// </summary>
        public BoneVM[] Roots { get; } = new BoneVM[1];

        /// <summary>
        /// the root node for this kinematic chain 
        /// </summary>
        public BoneVM Root { get { return Roots[0]; } }

        /// <summary>
        /// the currently selected bone
        /// </summary>
        public BoneVM SelectedItem
        {
            get { return selectedItem; }
            private set
            {
                if (selectedItem != value)
                {
                    if (selectedItem != null)
                    {
                        selectedItem.IsSelected = false;
                    }

                    selectedItem = value;

                    if (selectedItem != null)
                    {
                        selectedItem.IsSelected = true;
                    }

                    // HACK: this updates all commands. maybe we could be more selective
                    CommandManager.InvalidateRequerySuggested();
                    SetDetailItemRequested?.Invoke(selectedItem);
                }
            }
        }

        /// <summary>
        /// adds a new child to the currently selected node
        /// </summary>
        public ICommand AddBoneCommand { get; }

        /// <summary>
        /// remove the currently selected bone and all its children.
        /// </summary>
        public ICommand RemoveBoneCommand { get; }

        /// <summary>
        /// sets the currently selected item
        /// </summary>
        public ICommand ChangeSelectedCommand { get; }

        /// <summary>
        /// occurs when this instance tries to set the app-wide details item
        /// </summary>
        public event Action<object> SetDetailItemRequested;

        /// <summary>
        /// creates new kinematic instance
        /// </summary>
        /// <param name="model">the underlying model</param>
        /// <param name="sensors">collection of registered sensors.</param>
        public KinematicVM(KinematicStructure model)
        {
            Model = model;

            // create the bone ViewModel tree
            Roots[0] = new BoneVM(model.Root, null);

            foreach (BoneVM item in Root)
            {
                BoneVMMap.Add(item.Model, item);
            }

            // setup commands
            AddBoneCommand = new RelayCommand(AddBone, CanAddBone);
            RemoveBoneCommand = new RelayCommand(RemoveBone, CanRemoveBone);
            ChangeSelectedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(ChangeSelected);
        }

        /// <summary>
        /// sets the SelectedItem property called by the <see cref="ChangeSelectedCommand"/>
        /// </summary>
        private void ChangeSelected(RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = (BoneVM)e.NewValue;
        }

        /// <summary>
        /// creates and adds a new bone to the currently selected item
        /// </summary>
        private void AddBone()
        {
            if (SelectedItem == null)
                throw new InvalidOperationException("No bone selected!");

            var model = new Bone(parent: SelectedItem.Model, offset: new Vector3D(1, 0, 0));
            var vm = new BoneVM(model, parent: SelectedItem);
            SelectedItem.Model.Children.Add(model);
            SelectedItem.Children.Add(vm);
            BoneVMMap.Add(model, vm);
        }

        /// <summary>
        ///  can only add bones if the SelectedItem is set
        /// </summary>
        private bool CanAddBone()
        {
            return SelectedItem != null;
        }

        /// <summary>
        /// remove the currently selected bone
        /// </summary>
        private void RemoveBone()
        {
            if (SelectedItem == null)
                throw new InvalidOperationException("No bone selected!");
            if (SelectedItem.Parent == null)
                throw new InvalidOperationException("Can't remove root node");

            SelectedItem.Parent.Children.Remove(SelectedItem);
            BoneVMMap.Remove(SelectedItem.Model);
        }

        /// <summary>
        /// checks if the RemoveBone command can be executed
        /// </summary>
        private bool CanRemoveBone()
        {
            return SelectedItem != null && SelectedItem.Parent != null && !SelectedItem.IsLinkedToSensor;
        }

        /// <summary>
        /// Notifies the ui of changes in all child items
        /// </summary>
        public void Refresh()
        {
            foreach (var item in Roots)
                item.Refresh();
        }
    }
}

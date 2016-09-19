﻿using System;
using System.Drawing;
using System.Windows.Forms;
using CoBuilder.Service.Enums;
using CoBuilder.Service.Infrastructure.Config;
using CoBuilder.Service.Interfaces;

namespace CoBuilder.Service.GUI
{
    public partial class ConfigEditorDialog : Form
    {
        private readonly IConfiguration _configuration;
        private readonly IConfiguration _baseConfiguration;
        private ConfigEditState _configEditState;
        private OpenState _openState;

        #region Constructors

        public ConfigEditorDialog(IConfiguration configuration, IConfiguration baseConfiguration, OpenState state)
        {
            _configuration = configuration;
            _baseConfiguration = baseConfiguration;
            _openState = state;
            InitializeComponent();
        }

        public ConfigEditorDialog(IConfiguration configuration, IConfiguration baseConfiguration)
            : this(configuration, baseConfiguration, OpenState.Edit)
        {
        }

        public ConfigEditorDialog(IConfiguration baseConfiguration)
            : this(new Configuration(), baseConfiguration, OpenState.New)
        {
        }

        #endregion

        #region Porperties

        public IConfiguration Configuration { get; private set; }

        #endregion

        #region Event Handlers

        private void ConfigEditorDialog_Load(object sender, EventArgs e)
        {
            SwitchConfigEditState(ConfigEditState.Viewing);
            UpdateTreeView(TrvRoot, _baseConfiguration);
            UpdateTreeView(TrvConfiguration, _configuration);
        }

        private void cmdConfigEdit_Click(object sender, EventArgs e)
        {
            _configEditState = SwitchConfigEditState(_configEditState);
        }

        private void cmdConfigCancel_Click(object sender, EventArgs e)
        {
            _configEditState = SwitchConfigEditState(ConfigEditState.Cancel);
        }

        private void CmdAdd_Click(object sender, EventArgs e)
        {
            if (TrvRoot.SelectedNode == null) return;

            var definition = (IDefinition) TrvRoot.SelectedNode.Tag;
            TreeNode destinationNode;

            switch (definition.DefinitionType)
            {
                case DefinitionType.Property:
                    destinationNode = TrvConfiguration.SelectedNode == null || TrvConfiguration.SelectedNode.Level != 0
                        ? TrvConfiguration.TopNode.FirstNode
                        : TrvConfiguration.SelectedNode;
                    AddProperty(destinationNode, (PropertyDefinition) definition);
                    break;

                case DefinitionType.PropertySet:
                    destinationNode = TrvConfiguration.TopNode;
                    AddPropertySet(destinationNode, (PropertySetDefinition) definition);
                    break;

                case DefinitionType.Configuration:
                    return;

                default:
                    return;
            }

        }

        private void CmdEdit_Click(object sender, EventArgs e)
        {
            if (TrvConfiguration.SelectedNode == null || TrvConfiguration.SelectedNode.Level == 0) return;

            var editor = new DefinitionDialog((IDefinition) TrvConfiguration.SelectedNode.Tag);
            editor.ShowDialog();
        }

        private void CmdRemove_Click(object sender, EventArgs e)
        {
            if (TrvConfiguration.SelectedNode == null || TrvConfiguration.SelectedNode.Level == 0) return;

            var definition = (IDefinition) TrvConfiguration.SelectedNode.Tag;
            var parentDefinition = (IDefinition) TrvConfiguration.SelectedNode.Parent.Tag;

            switch (parentDefinition.DefinitionType)
            {
                case DefinitionType.PropertySet:
                    var pSet = (IPropertySetDefinition) parentDefinition;
                    var property = (IPropertyDefinition) definition;
                    pSet.RemoveProperty(property);
                    TrvConfiguration.SelectedNode.Remove();
                    break;

                case DefinitionType.Configuration:
                    var root = (IConfigDefinition) parentDefinition;
                    var pSetRemove = (IPropertySetDefinition) definition;
                    root.RemovePropertySet(pSetRemove);
                    TrvConfiguration.SelectedNode.Remove();
                    break;

                default:
                    return;

            }
        }

        private void CmbOK_Click(object sender, EventArgs e)
        {
            Configuration = _configuration.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CmbCancel_Click(object sender, EventArgs e)
        {
            Configuration = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Methods

        private ConfigEditState SwitchConfigEditState(ConfigEditState state)
        {
            switch (state)
            {
                case ConfigEditState.Editing:
                    if (tbxName.Text == "")
                    {
                        MessageBox.Show("Name is Required");
                        return state;
                    }
                    tbxName.Visible = false;
                    _configuration.Name = tbxName.Text;
                    tbxAuthor.Visible = false;
                    _configuration.Author = tbxAuthor.Text;

                    lblNameValue.Visible = true;
                    lblNameValue.Text = _configuration.Name;
                    lblAuthorValue.Visible = true;
                    lblAuthorValue.Text = _configuration.Author;

                    cmdConfigEdit.Text = "&Edit";
                    cmdConfigEdit.Location = new Point(cmdConfigEdit.Location.X + 55, cmdConfigEdit.Location.Y);
                    cmdConfigCancel.Visible = false;

                    SpCForm.Enabled = true;
                    CmbOK.Enabled = true;
                    return ConfigEditState.Viewing;

                case ConfigEditState.Viewing:

                    tbxName.Visible = true;
                    tbxName.Text = _configuration.Name;
                    tbxAuthor.Visible = true;
                    tbxAuthor.Text = _configuration.Author;

                    lblNameValue.Visible = false;
                    lblAuthorValue.Visible = false;

                    cmdConfigEdit.Text = "&Save";
                    cmdConfigEdit.Location = new Point(cmdConfigEdit.Location.X - 55, cmdConfigEdit.Location.Y);
                    cmdConfigCancel.Visible = true;

                    SpCForm.Enabled = false;
                    CmbOK.Enabled = false;
                    tbxName.Focus();
                    return ConfigEditState.Editing;

                case ConfigEditState.Cancel:

                    tbxName.Visible = false;
                    tbxAuthor.Visible = false;

                    lblNameValue.Visible = true;
                    lblNameValue.Text = _configuration.Name;
                    lblAuthorValue.Visible = true;
                    lblAuthorValue.Text = _configuration.Author;

                    cmdConfigEdit.Text = "&Edit";
                    cmdConfigEdit.Location = new Point(cmdConfigEdit.Location.X + 55, cmdConfigEdit.Location.Y);
                    cmdConfigCancel.Visible = false;

                    SpCForm.Enabled = true;
                    CmbOK.Enabled = true;
                    return ConfigEditState.Viewing;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, "Invalid State Arguement Supplied");
            }
        }

        private void AddProperty(TreeNode destinationNode, PropertyDefinition definition)
        {
            var pSet = (PropertySetDefinition) destinationNode.Tag;
            var copy = definition.DeepCopy();
            pSet.AddProperty(copy);

            TrvConfiguration.BeginUpdate();
            var propNode = destinationNode.Nodes.Add(copy.Identifier, copy.DisplayName);
            propNode.Tag = copy;

            TrvConfiguration.EndUpdate();
        }

        private void AddPropertySet(TreeNode destinationNode, PropertySetDefinition definition)
        {
            var root = (ConfigDefinition) destinationNode.Tag;
            var copy = definition.DeepCopy();
            root.AddPropertySet(copy);

            TrvConfiguration.BeginUpdate();

            var pSetNode = destinationNode.Nodes.Add(copy.Identifier, copy.DisplayName);
            pSetNode.Tag = copy;

            foreach (var property in copy.Properties.Values)
            {
                var propNode = pSetNode.Nodes.Add(property.Identifier, property.DisplayName);
                propNode.Tag = property;
            }

            TrvConfiguration.EndUpdate();
        }

        private static void UpdateTreeView(TreeView treeView, IConfiguration config)
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var root = treeView.Nodes.Add(config.Root.Identifier, config.Root.DisplayName);
            root.Tag = config.Root;

            foreach (var pSet in config.Root.PropertySets.Values)
            {
                var pSetNode = root.Nodes.Add(pSet.Identifier, pSet.DisplayName);
                pSetNode.Tag = pSet;

                foreach (var property in pSet.Properties.Values)
                {
                    var propNode = pSetNode.Nodes.Add(property.Identifier, property.DisplayName);
                    propNode.Tag = property;
                }
            }

            treeView.EndUpdate();
        }

        #endregion
    }
}
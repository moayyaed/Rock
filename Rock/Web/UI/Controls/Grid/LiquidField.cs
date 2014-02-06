﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:LiquidField runat=server></{0}:LiquidField>" )]
    public class LiquidField : TemplateField
    {
        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField" />.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            LiquidFieldTemplate liquidFieldTemplate = new LiquidFieldTemplate();
            this.ItemTemplate = liquidFieldTemplate;
            this.ParentGrid = control as Grid;
            return base.Initialize( sortingEnabled, control );
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

        /// <summary>
        /// Gets or sets the liquid template.
        /// </summary>
        /// <value>
        /// The liquid template.
        /// </value>
        public string LiquidTemplate { get; set; }

        /// <summary>
        /// Gets or sets the liquid key, for example: Person
        /// </summary>
        /// <value>
        /// The liquid key.
        /// </value>
        public string LiquidKey { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LiquidFieldTemplate : ITemplate
    {
        /// <summary>
        /// Gets or sets the liquid field.
        /// </summary>
        /// <value>
        /// The liquid field.
        /// </value>
        private LiquidField LiquidField { get; set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            DataControlFieldCell cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                this.LiquidField = cell.ContainingField as LiquidField;
                Literal lOutputText = new Literal();
                lOutputText.DataBinding += lOutputText_DataBinding;
                cell.Controls.Add( lOutputText );
            }
        }

        /// <summary>
        /// Handles the DataBinding event of the lOutputText control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lOutputText_DataBinding( object sender, EventArgs e )
        {
            Literal lOutputText = sender as Literal;
            GridViewRow gridViewRow = lOutputText.NamingContainer as GridViewRow;
            if ( gridViewRow.DataItem != null )
            {
                Dictionary<string, object> mergeValues = new Dictionary<string, object>();
                mergeValues.Add( this.LiquidField.LiquidKey, this.ToGridItemsDictionary( gridViewRow, gridViewRow.DataItem ) );
                lOutputText.Text = this.LiquidField.LiquidTemplate.ResolveMergeFields( mergeValues );
            }
        }

        /// <summary>
        /// Gets or sets the data item properties.
        /// </summary>
        /// <value>
        /// The data item properties.
        /// </value>
        private Dictionary<string, System.Reflection.PropertyInfo> DataItemPropertiesDictionary { get; set; }

        /// <summary>
        /// To the dictionary.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        private Dictionary<string, object> ToGridItemsDictionary( GridViewRow gridViewRow, object dataItem )
        {
            var dictionary = new Dictionary<string, object>();

            if ( this.DataItemPropertiesDictionary == null )
            {
                PopulateDataItemPropertiesDictionary( dataItem );
            }

            foreach ( var propInfo in DataItemPropertiesDictionary )
            {
                dictionary.Add( propInfo.Key, propInfo.Value.GetValue( dataItem, null ) );
            }

            return dictionary;
        }

        /// <summary>
        /// Populates the data item properties dictionary.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        private void PopulateDataItemPropertiesDictionary( object dataItem )
        {
            var dataItemProperties = dataItem.GetType().GetProperties().Where( a => !a.GetGetMethod().IsVirtual || a.Name == "Id" || a.Name == "Guid" || a.Name == "Order" ).ToArray();
            this.DataItemPropertiesDictionary = new Dictionary<string, System.Reflection.PropertyInfo>();

            // add MergeFields based on the associated ColumnHeaderText of each property of the dataitem (without spaces or special chars)
            foreach ( var itemPropInfo in dataItemProperties )
            {
                var gridField = LiquidField.ParentGrid.Columns.OfType<BoundField>().FirstOrDefault( a => a.DataField == itemPropInfo.Name );
                if ( gridField != null )
                {
                    var mergeFieldName = gridField.HeaderText.Replace( " ", string.Empty ).RemoveSpecialCharacters();
                    this.DataItemPropertiesDictionary.Add( mergeFieldName, itemPropInfo );
                }
                else
                {
                    // add properties that aren't shown in the grid in the next loop
                }
            }

            // add additional MergeFields for Properties of the dataitem that aren't already already a MergeField created from the ColumnHeaderText
            foreach ( var itemPropInfo in dataItemProperties )
            {
                var mergeFieldName = itemPropInfo.Name;
                if ( !this.DataItemPropertiesDictionary.ContainsKey( mergeFieldName ) )
                {
                    this.DataItemPropertiesDictionary.Add( mergeFieldName, itemPropInfo );
                }
            }
        }
    }
}
﻿using Google.Apis.Sheets.v4.Data;
using sbdotnet;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;

namespace sbGoogleApi.Sheets
{
    /// <summary>
    /// Parses a Grid-type Google Sheet into a DataTable.
    /// The View property is for databinding. Additional views
    /// can also be made from the DataTable Table property.
    /// Assumptions: 
    /// - The sheet in question has a header row of column names
    /// - Data rows are consistent in their types, per column
    /// </summary>
    public class GSheetDataTable : INotifyPropertyChanged
    {
        ///////////////////////////////////////////////////////////
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<TField>(ref TField field, TField value, string propertyName)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        #endregion INotifyPropertyChanged
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Fields

        private string _Name = string.Empty;
        private string _Description = string.Empty;
        private GridData _Data;
        private DataTable _Table;
        private DataView _View;

        #endregion Fields
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Properties

        public string Name
        {
            get => _Name;
            set => SetField(ref _Name, value, nameof(Name));
        }

        public string Description
        {
            get => _Description;
            set => SetField(ref _Description, value, nameof(Description));
        }

        public GridData Data
        {
            get => _Data;
            set => SetField(ref _Data, value, nameof(Data));
        }

        public DataTable Table
        {
            get => _Table;
            set => SetField(ref _Table, value, nameof(Table));
        }

        public DataView View
        {
            get => _View;
            set => SetField(ref _View, value, nameof(View));
        }

        #endregion Properties
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Interface

        public GSheetDataTable(GridData data, string? name, string? description)
        {
            _Data = data;
            _Name = name ?? string.Empty;
            _Description = description ?? string.Empty;
            _Table = new();
            _View = _Table.DefaultView;
        }

        public void Parse()
        {
            try
            {
                int RowCount = Data.RowData.Count;
                if (RowCount < 2)
                {
                    Logger.Warning("Sheet does not seem to contain a header row and at least one data row");
                    return;
                }

                // Pass 1: columns
                var row0 = Data.RowData[0];
                var row1 = Data.RowData[1];
                for (int i = 0; i < row1.Values.Count; i++)
                {
                    ExtendedValue ev_header = row0.Values[i].EffectiveValue;
                    ExtendedValue ev_data = row1.Values[i].EffectiveValue;

                    if (ev_data.BoolValue is not null)
                    {
                        Table.Columns.Add(ev_header.StringValue, typeof(bool));
                    }
                    else if (ev_data.NumberValue is not null)
                    {
                        Table.Columns.Add(ev_header.StringValue, typeof(double));
                    }
                    else if (ev_data.StringValue is not null)
                    {
                        Table.Columns.Add(ev_header.StringValue, typeof(string));
                    }
                    else
                    {
                        Table.Columns.Add(ev_header.StringValue, typeof(object));
                        Logger.Warning($"Data type of column {i} is not accounted for. Defaulting to object");
                    }
                }
                
                // Pass 2: DataRows from sheet rows
                for (int i = 1; i < Data.RowData.Count; i++)
                {
                    var grow = Data.RowData[i].Values;
                    List<object?> values = [];
                    foreach (var value in grow)
                    {
                        if (value.EffectiveValue.BoolValue is not null)
                        {
                            values.Add(value.EffectiveValue.BoolValue);
                        }
                        else if (value.EffectiveValue.NumberValue is not null)
                        {
                            values.Add(value.EffectiveValue.NumberValue);
                        }
                        else if (value.EffectiveValue.StringValue is not null)
                        {
                            values.Add(value.EffectiveValue.StringValue);
                        }
                        else
                        {
                            values.Add(value.EffectiveValue.ToString());
                        }
                    }
                    Table.Rows.Add([.. values]);
                }

                View = Table.AsDataView();
            }
            catch (Exception ex)
            {
                sbdotnet.Logger.Error(ex);
            }
        }

        #endregion Interface
        ///////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////
        #region Internal

        #endregion Internal
        ///////////////////////////////////////////////////////////
    }
}

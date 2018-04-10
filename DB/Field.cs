using System;


namespace SB1Util.DB
{
    public class Field
    {
        public enum FieldType
        {
            PRIMITIVE, SAP, USER
        }

        public enum FieldDataType
        {
            INT, DOUBLE, STRING, DATE
        }

        private string formFieldName;
        private string dbFieldName;
        private string clsFieldName;
        private FieldType type;
        private FieldDataType dataType;

        public Field(string dbFName, string clsFName, string formField, FieldType type, FieldDataType dataType)
        {
            this.clsFieldName = clsFName;
            this.dbFieldName = dbFName;
            this.formFieldName = formField;
            this.type = type;
            this.dataType = dataType;
        }


        public string DBFieldName
        {
            get { return dbFieldName; }
        }

        public string ClsFieldName
        {
            get { return clsFieldName; }
        }

        public string FormFieldName
        {
            get { return formFieldName; }
        }

        public FieldType Type
        {
            get { return type; }
        }

        public FieldDataType DataType
        {
            get { return dataType; }
        }

    }
}

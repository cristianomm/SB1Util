using System;
using System.Collections.Generic;
using System.Text;

namespace SB1Util.DB
{
    public class ObjectMap
    {

        private string userTable;
        private string className;
        private string formID;
        private LinkedList<Field> fields;


        public ObjectMap(string className, string userTable, string formID)
        {
            this.formID = formID;
            this.className = className;
            this.userTable = userTable;
            this.fields = new LinkedList<Field>();
        }


        public void addField(string tableField, string classField, string formField, Field.FieldType type, Field.FieldDataType dataType) 
        {
            fields.AddLast(new Field(tableField, classField, formField, type, dataType));
        }


        public string UserTable
        {
            get { return userTable; }
        }
        
        public string ClassName
        {
            get { return className; }
        }

        public string FormID
        {
            get { return formID; }
        }


    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSQLiteAndPostgreSQLWPF.dao
{
    class DBResult
    {
        String columName;
        String fieltType;
        String dataTypeName;
        String value;

        public DBResult(String columName, String fieltType, String dataTypeName, String value)
        {
            this.columName = columName;
            this.fieltType = fieltType;
            this.dataTypeName = dataTypeName;
            this.value = value;
        }
    }
}

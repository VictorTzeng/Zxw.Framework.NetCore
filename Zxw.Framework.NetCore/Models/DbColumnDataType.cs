using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using MongoDB.Driver;
using NpgsqlTypes;

namespace Zxw.Framework.NetCore.Models
{
    public class DbColumnDataType
    {
        public DatabaseType DatabaseType { get; set; }

        public string ColumnTypes { get; set; }

        public string CSharpType { get; set; }
    }

    public enum DatabaseType
    {
        MSSQL,
        MySQL,
        PostgreSQL,
        SQLite,
        InMemory,
        Oracle,
        MariaDB,
        MyCat,
        Firebird,
        DB2,
        Access
    }

    public class DbColumnTypeCollection
    {
        public static IList<DbColumnDataType> DbColumnDataTypes=>new List<DbColumnDataType>()
        {
            #region MSSQL，https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings

            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "bigint", CSharpType = "Int64"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "binary,image,varbinary(max),rowversion,timestamp,varbinary", CSharpType = "Byte[]"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "bit", CSharpType = "Boolean"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "char,nchar,text,ntext,varchar,nvarchar", CSharpType = "String"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "date,datetime,datetime2,smalldatetime", CSharpType ="DateTime"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "datetimeoffset", CSharpType ="DateTimeOffset"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "decimal,money,numeric,smallmoney", CSharpType ="Decimal"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "float", CSharpType ="Double"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "int", CSharpType ="Int32"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "real", CSharpType ="Single"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "smallint", CSharpType ="Int16"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "sql_variant", CSharpType ="Object"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "time", CSharpType ="TimeSpan"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "tinyint", CSharpType ="Byte"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MSSQL, ColumnTypes = "uniqueidentifier", CSharpType ="Guid"},

            #endregion

            #region PostgreSQL，http://www.npgsql.org/doc/types/basic.html

            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "boolean,bit(1)", CSharpType ="Boolean"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "smallint", CSharpType ="short"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "integer", CSharpType ="int"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "bigint", CSharpType ="long"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "real", CSharpType ="float"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "double precision", CSharpType ="Double"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "numeric,money", CSharpType ="decimal"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "text,character,character varying,citext,json,jsonb,xml,name", CSharpType ="String"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "point", CSharpType ="NpgsqlTypes.NpgsqlPoint"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "lseg", CSharpType ="NpgsqlTypes.NpgsqlLSeg"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "path", CSharpType ="NpgsqlTypes.NpgsqlPath"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "polygon", CSharpType ="NpgsqlTypes.NpgsqlPolygon"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "line", CSharpType ="NpgsqlTypes.NpgsqlLine"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "circle", CSharpType ="NpgsqlTypes.NpgsqlCircle"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "box", CSharpType ="NpgsqlTypes.NpgsqlBox"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "bit(n),bit varying", CSharpType ="BitArray"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "hstore", CSharpType ="IDictionary<string, string>"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "uuid", CSharpType ="Guid"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "cidr", CSharpType ="ValueTuple<IPAddress, int>"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "inet", CSharpType ="IPAddress"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "macaddr", CSharpType ="PhysicalAddress"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "tsquery", CSharpType ="NpgsqlTypes.NpgsqlTsQuery"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "tsvector", CSharpType ="NpgsqlTypes.NpgsqlTsVector"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "date,timestamp,timestamp with time zone,timestamp without time zone", CSharpType ="DateTime"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "interval,time", CSharpType ="TimeSpan"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "time with time zone", CSharpType ="DateTimeOffset"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "bytea", CSharpType ="Byte[]"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "oid,xid,cid", CSharpType ="uint"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "oidvector", CSharpType ="uint[]"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "char", CSharpType ="char"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "geometry", CSharpType ="NpgsqlTypes.PostgisGeometry"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.PostgreSQL, ColumnTypes = "record", CSharpType ="object[]"},

            #endregion

            #region MySQL，https://www.devart.com/dotconnect/mysql/docs/DataTypeMapping.html

            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "bool,boolean,bit(1),tinyint(1)", CSharpType ="Boolean"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "tinyint", CSharpType ="SByte"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "tinyint unsigned", CSharpType ="Byte"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "smallint, year", CSharpType ="Int16"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "int, integer, smallint unsigned, mediumint", CSharpType ="Int32"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "bigint, int unsigned, integer unsigned, bit", CSharpType ="Int64"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "float", CSharpType ="Single"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "double, real", CSharpType ="Double"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "decimal, numeric, dec, fixed, bigint unsigned, float unsigned, double unsigned, serial", CSharpType ="Decimal"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "date, timestamp, datetime", CSharpType ="DateTime"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "datetimeoffset", CSharpType ="DateTimeOffset"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "time", CSharpType ="TimeSpan"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "char, varchar, tinytext, text, mediumtext, longtext, set, enum, nchar, national char, nvarchar, national varchar, character varying", CSharpType ="String"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "binary, varbinary, tinyblob, blob, mediumblob, longblob, char byte", CSharpType ="Byte[]"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "geometry", CSharpType ="System.Data.Spatial.DbGeometry"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.MySQL, ColumnTypes = "geometry", CSharpType ="System.Data.Spatial.DbGeography"},

            #endregion

            #region Oracle, https://docs.oracle.com/cd/E14435_01/win.111/e10927/featUDTs.htm#CJABAEDD

            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "BFILE,BLOB,NCLOB,CLOB,REFCURSOR", CSharpType ="Object"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "CHAR,NCHAR,VARCHAR2,NVARCHAR2,XMLTYPE,ROWID,LONG", CSharpType ="String"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "Byte", CSharpType ="Byte"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "LongRaw,Raw", CSharpType ="Binary"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "Decimal", CSharpType ="Decimal"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "Single", CSharpType ="Single"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "Double,FLOAT", CSharpType ="Double"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "Int16", CSharpType ="Int16"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "Int32", CSharpType ="Int32"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "Int64,IntervalYM", CSharpType ="Int64"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "DATE, TIMESTAMP, TimeStampLTZ,TimeStampTZ,TIMESTAMP(0),TIMESTAMP(1),TIMESTAMP(2),TIMESTAMP(3),TIMESTAMP(4),TIMESTAMP(5),TIMESTAMP(6),TIMESTAMP(7),TIMESTAMP(8),TIMESTAMP(9)", CSharpType ="DateTime"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "IntervalDS,INTERVAL DAY TO SECOND", CSharpType ="TimeSpan"},
            new DbColumnDataType(){ DatabaseType = DatabaseType.Oracle, ColumnTypes = "NUMBER", CSharpType ="Decimal"},

            #endregion
        };
    }
}

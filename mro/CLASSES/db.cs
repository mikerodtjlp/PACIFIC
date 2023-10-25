#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Data;
using System.Configuration;
using System.Reflection;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data.OleDb;
using System.Collections.Generic;

namespace mro.db {
  // =========================================================================
  // Abstract Class definition.. hold all the methods
  // =========================================================================
  public abstract class Database {
    public string connectionString;
    public abstract IDbConnection CreateConnection();
    public abstract IDbCommand CreateCommand();
    public abstract IDbConnection CreateOpenConnection();
    public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection);
    public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction);
    //public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection);
    public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection);
    public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection, IDbTransaction transaction);
    public abstract IDataParameter CreateParameter(string parameterName, object parameterValue);
  }

  // =========================================================================
  // Accessing configuration files
  // =========================================================================
  /*public sealed class DatabaseFactorySectionHandler : ConfigurationSection
  {
      [ConfigurationProperty("Name")]
      public string Name
      {
          get { return (string)base["Name"]; }
      }

      [ConfigurationProperty("ConnectionStringName")]
      public string ConnectionStringName
      {
          get { return (string)base["ConnectionStringName"]; }
      }

      public string getConnectionString(string name)
      {
          try
          {
              return ConfigurationManager.ConnectionStrings[name].ConnectionString;
          }
          catch (Exception excep)
          {
              throw new Exception(string.Concat(  "Connection string ", 
                                                  name, 
                                                  " was not found in web.config. ",
                                                  excep.Message));
          }
      }

      //public string ConnectionString
      //{
      //    get
      //    {
      //        try
      //        {
      //            return ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
      //        }
      //        catch (Exception excep)
      //        {
      //            throw new Exception("Connection string " + ConnectionStringName + " was not found in web.config. " + excep.Message);
      //        }
      //    }
      //}
  }*/


  // =========================================================================
  // Factory of the connection string
  // =========================================================================
  /*public sealed class DatabaseFactory
  {
      public static Dictionary<string, DatabaseFactorySectionHandler> handlers = 
          new Dictionary<string, DatabaseFactorySectionHandler>();
      public static Dictionary<string, ConstructorInfo> ctors = 
          new Dictionary<string, ConstructorInfo>();

      private DatabaseFactory() { }

      public static Database CreateDatabase(string namedb)
      {
          if(string.IsNullOrEmpty(namedb)) err.require("empty_connection_string");
          string factory = string.Empty;

          // too restrictive if less than 32 cars, then we take the namedb as a entry on the 
          // webcofing else we take as a full connection string, this needs to be work out 
          if (namedb.Length > 32)
          {
              int sep = namedb.IndexOf('|');
              if (sep != -1)
              {
                  factory = namedb.Substring(sep + 1);
                  namedb = namedb.Substring(0, sep);
              }
          }
          if (factory.Length == 0) factory = "corefactory";

          DatabaseFactorySectionHandler sectionHandler = null;
          ConstructorInfo constructor = null;
          if (!handlers.TryGetValue(factory, out sectionHandler))
          {
              sectionHandler = (DatabaseFactorySectionHandler)
                              ConfigurationManager.GetSection(factory);
              handlers.Add(factory, sectionHandler);
              // Find the class
              Type database = Type.GetType(sectionHandler.Name);
              // Get it's constructor
              constructor = database.GetConstructor(new Type[] { });
              ctors.Add(factory, constructor);
          }
          else if (!ctors.TryGetValue(factory, out constructor)) 
              err.require("db_factory_missing");

          // Verify a DatabaseFactoryConfiguration line exists in the web.config.
          if (sectionHandler.Name.Length == 0)
              err.require("db_entry_not_defined_in_corefactory_section_of_web.config");

          try
          {
              // Invoke it's constructor, which returns an instance.
              Database createdObject = (Database)constructor.Invoke(null);

              // Initialize the connection string property for the database.
              createdObject.connectionString = namedb.Length > 32 ? namedb : 
                                  sectionHandler.getConnectionString(namedb);

              return createdObject;
          }
          catch (Exception excep)
          {
              throw new Exception(sectionHandler.Name, excep);
          }
      }
  }*/


  // =========================================================================
  // Create the connections instance objects 
  // =========================================================================
  public class DataWorker {
    private Database _database = null;

    public DataWorker(string name) {
      try {
        //_database = DatabaseFactory.CreateDatabase(name);
        _database = new sqlDatabase();
        _database.connectionString = name;
      } 
      catch (Exception excep) {
        throw excep;
      }
    }

    public Database database {
      get { return _database; }
    }
  }

  // =========================================================================
  // Create the connections instance objects (SQL Server)
  // =========================================================================
  public class sqlDatabase : Database {
    public override IDbConnection CreateConnection() {
      return new SqlConnection(connectionString);
    }

    public override IDbCommand CreateCommand() {
      return new SqlCommand();
    }

    public override IDbConnection CreateOpenConnection() {
      SqlConnection connection = (SqlConnection)CreateConnection();
      connection.Open();
      return connection;
    }

    public override IDbCommand CreateCommand(string commandText,
                                                IDbConnection connection) {
      SqlCommand command = (SqlCommand)CreateCommand();
      command.CommandText = commandText;
      command.Connection = (SqlConnection)connection;
      command.CommandType = CommandType.Text;
      return command;
    }

    public override IDbCommand CreateStoredProcCommand(string procName,
                                                        IDbConnection connection) {
      SqlCommand command = (SqlCommand)CreateCommand();
      command.CommandText = procName;
      command.Connection = (SqlConnection)connection;
      command.CommandType = CommandType.StoredProcedure;
      return command;
    }

    public override IDataParameter CreateParameter(string parameterName,
                                                        object parameterValue) {
      return new SqlParameter(parameterName, parameterValue);
    }

    public override IDbCommand CreateStoredProcCommand(string procName,
                                                        IDbConnection connection,
                                                        IDbTransaction transaction) {
      SqlCommand command = (SqlCommand)CreateCommand();
      command.CommandText = procName;
      command.Connection = (SqlConnection)connection;
      command.CommandType = CommandType.StoredProcedure;
      if (transaction != null)
        command.Transaction = (SqlTransaction)transaction;
      return command;
    }

    public override IDbCommand CreateCommand(string commandText,
                                                IDbConnection connection,
                                                IDbTransaction transaction) {
      SqlCommand command = (SqlCommand)CreateCommand();
      command.CommandText = commandText;
      command.Connection = (SqlConnection)connection;
      command.CommandType = CommandType.Text;
      command.Transaction = (SqlTransaction)transaction;
      return command;
    }
  }

  public class accessDatabase : Database {
    public override IDbConnection CreateConnection() {
      return new OleDbConnection(connectionString);
    }

    public override IDbCommand CreateCommand() {
      return new OleDbCommand();
    }

    public override IDbConnection CreateOpenConnection() {
      OleDbConnection connection = (OleDbConnection)CreateConnection();
      connection.Open();
      return connection;
    }

    public override IDbCommand CreateCommand(string commandText,
                                                IDbConnection connection) {
      OleDbCommand command = (OleDbCommand)CreateCommand();
      command.CommandText = commandText;
      command.Connection = (OleDbConnection)connection;
      command.CommandType = CommandType.Text;
      return command;
    }

    public override IDbCommand CreateStoredProcCommand(string procName,
                                                        IDbConnection connection) {
      SqlCommand command = (SqlCommand)CreateCommand();
      command.CommandText = procName;
      command.Connection = (SqlConnection)connection;
      command.CommandType = CommandType.StoredProcedure;
      return command;
    }

    public override IDataParameter CreateParameter(string parameterName,
                                                    object parameterValue) {
      return new OleDbParameter(parameterName, parameterValue);
    }

    public override IDbCommand CreateStoredProcCommand(string procName,
                                                        IDbConnection connection,
                                                        IDbTransaction transaction) {
      OleDbCommand command = (OleDbCommand)CreateCommand();

      command.CommandText = procName;
      command.Connection = (OleDbConnection)connection;
      command.CommandType = CommandType.StoredProcedure;
      if (transaction != null) {
        command.Transaction = (OleDbTransaction)transaction;
      }
      return command;
    }

    public override IDbCommand CreateCommand(string commandText,
                                                IDbConnection connection,
                                                IDbTransaction transaction) {
      OleDbCommand command = (OleDbCommand)CreateCommand();
      command.CommandText = commandText;
      command.Connection = (OleDbConnection)connection;
      command.CommandType = CommandType.Text;
      command.Transaction = (OleDbTransaction)transaction;
      return command;
    }
  }

  public sealed class validate {
    //Singleton initiation
    private static validate newvalidate = new validate();
    public static validate getInstance() {
      return newvalidate;
    }

    #region General Methods
    public bool isNumeric(string myNumber) {
      bool IsNum = false;
      for (int index = 0; index < myNumber.Length; index++) {
        IsNum = true;
        if (!Char.IsNumber(myNumber[index])) {
          IsNum = false;
          break;
        }
      }
      return IsNum;
    }
    public object getDefaultIfNull(string obj, TypeCode typeCode) {
      //If object is dbnull then return the default for that type.
      //Otherwise just return the orginal value.
      object obj2 = obj;
      if (obj.Length == 0) {
        switch (typeCode) {
          case TypeCode.Int32: obj2 = 0; break;
          case TypeCode.Double: obj2 = 0; break;
          case TypeCode.String: obj2 = string.Empty; break;
          case TypeCode.Boolean: obj2 = false; break;
          case TypeCode.DateTime: obj2 = new DateTime(); break;
          case TypeCode.Int64: obj2 = 0; break;
          default: break;
        }
      }
      return obj2;
    }
    #endregion

    #region DAL Methods

    /**
   * Checks if an object coming back from the database is dbnull.  If it is this returns the default
   * value for that type of object.
   * <param name="obj">Object to check for null.</param>
   * <param name="typeCode">Type of object, used to determine what the default value is.</param>
   * <returns>Either the object passed in or the default value.</returns>
   */
    public static object getDefaultIfDBNull(object obj, TypeCode typeCode) {
      // If object is dbnull then return the default for that type.
      // Otherwise just return the orginal value.
      if (obj == DBNull.Value) {
        switch (typeCode) {
          case TypeCode.Int32: obj = 0; break;
          case TypeCode.Double: obj = 0; break;
          case TypeCode.String: obj = string.Empty; break;
          case TypeCode.Boolean: obj = false; break;
          case TypeCode.DateTime: obj = new DateTime(); break;
          case TypeCode.Int64: obj = 0; break;
          default: break;
        }
      }
      return obj;
    }

    /**
   * Evaluate the parameters list and assign DBNull as apropiated
   */
    public static void evaluateParameters(IDataParameterCollection parameters,
                                           bool evaluateNumeric) {
      foreach (IDataParameter Parameter in parameters) {
        if (Parameter.Value == null ||
           Convert.ToString(Parameter.Value).Length == 0) {
          Parameter.Value = DBNull.Value;
        }
        else {
          if (evaluateNumeric && (string.Equals(Convert.ToString(Parameter.Value), "0") ||
                            string.Equals(Convert.ToString(Parameter.Value), "0.0"))) {
            Parameter.Value = DBNull.Value;
          }
        }
      }
    }

    public Object setDBNull(String str) {
      return (str == null || str.Equals(String.Empty)) ? (Object)DBNull.Value : str;
    }

    public static bool isvalidDate(DateTime d) {
      return d != DateTime.MinValue;
    }
    #endregion
  }
}

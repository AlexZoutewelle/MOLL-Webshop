using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using MySql.Data.MySqlClient;
using System.Data;

namespace TestWebApp.Models
{
    public static class ProcedureCall<R>
    {
        public static MySqlConnection conn;
        public static string[] inputKeys;
        public static string[] outputKeys;
        public static int[] outputKeysPositions;


        public static MySqlConnection GetConnection()
        {
            conn = new MySqlConnection("server=localhost;port=3306;Database=sys;Uid=root;Pwd=admin");
            return conn;
        }

        public static MySqlParameter[] PrepareParameters(Dictionary<string, Object> dic)
        {
            inputKeys = dic.Keys.Where(k => k.StartsWith("in")).ToArray();
            outputKeys = dic.Keys.Where(k => k.StartsWith("out")).ToArray();
            outputKeysPositions = new int[outputKeys.Length];

            MySqlParameter[] parameters = new MySqlParameter[dic.Count];

            //set input variables
            for (int i = 0; i < inputKeys.Length; i++)
            {
                string currentKey = inputKeys[i];
                Object currentValue = dic[currentKey];
                parameters[i] = new MySqlParameter(inputKeys[i], RetreiveDbType(currentValue));
                parameters[i].Direction = ParameterDirection.Input;
                parameters[i].Value = dic[currentKey];
            }

            //set output variables
            if (outputKeys.Length > 0)
            {
                for (int i = inputKeys.Length; i < parameters.Length; i++)
                {
                    string currentKey = outputKeys[i - inputKeys.Length];
                    Object currentValue = dic[currentKey];
                    parameters[i] = new MySqlParameter(currentKey, RetreiveDbType(currentValue));
                    parameters[i].Direction = ParameterDirection.Output;
                    parameters[i].Value = dic[currentKey];

                    outputKeysPositions[i - inputKeys.Length] = i;
                }
            }
            return parameters;
        }

        public static MySqlCommand prepareCommand(string procedureName, MySqlParameter[] parameters)
        {
            MySqlCommand cmd = new MySqlCommand(procedureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(parameters);

            return cmd;
        }

        public static Dictionary<string,Object> ExecuteNonQuery(Dictionary<string, Object> dic, string procedureName)
        {
            MySqlParameter[] parameters = PrepareParameters(dic);
            List<Object> returnParameters = new List<Object>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = prepareCommand(procedureName, parameters);

                    cmd.ExecuteNonQuery();

                    for(int i = 0; i < outputKeys.Length; i++)
                    {
                        dic[outputKeys[i]] = parameters[outputKeysPositions[i]].Value;
                    }                        
                    return dic;
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }



        public static R ExecuteReader(Dictionary<string, Object> dic, string procedureName)
        {
            inputKeys = dic.Keys.Where(k => k.StartsWith("in")).ToArray();
            outputKeys = dic.Keys.Where(k => k.StartsWith("out")).ToArray();

            MySqlParameter[] parameters = PrepareParameters(dic);
            R objectToFill = ConstructObjectToFill();

            try
            {
                using (MySqlConnection conn = GetConnection() )
                {
                    MySqlCommand cmd = prepareCommand(procedureName, parameters);
                    conn.Open();

                    MySqlDataReader reader = cmd.ExecuteReader();
                    objectToFill = FillObject(reader, objectToFill);
                    return objectToFill;
                }}
            catch (Exception e)
            {
                return objectToFill;
            }
        }

        public static MySqlDbType RetreiveDbType(Object givenObject)
        {
            MySqlDbType typeToReturn = MySqlDbType.Bit;

            if (givenObject.GetType() == typeof(int))
            {
                typeToReturn = MySqlDbType.Int32;
            }

            if (givenObject.GetType() == typeof(string))
            {
                typeToReturn = MySqlDbType.VarChar;
            }

            if (givenObject.GetType() == typeof(DateTime))
            {
                typeToReturn = MySqlDbType.DateTime;
            }

            if (givenObject.GetType() == typeof(TimeSpan))
            {
                typeToReturn = MySqlDbType.Time;
            }
            return typeToReturn;
        }

        public static R ConstructObjectToFill()
        {
            Type thing = typeof(R);
            R objectToFill = (R)Activator.CreateInstance(thing);
            return objectToFill;
        }

        public static R FillObject(MySqlDataReader resultSet, Object objectToFill)
        {
            PropertyInfo[] properties = typeof(R).GetProperties();

            while (resultSet.Read())
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    string propertyName = properties[i].Name;

                    if(propertyName == "Message")
                    {
                        break;
                    }

                    if (properties[i].PropertyType == typeof(string))
                    {
                        string valueToSet = resultSet.GetString(propertyName);
                        properties[i].SetValue(objectToFill, valueToSet);
                    }

                    if (properties[i].PropertyType == typeof(int))
                    {
                        int valueToSet = resultSet.GetInt32(propertyName);
                        properties[i].SetValue(objectToFill, valueToSet);
                    }

                    if (properties[i].PropertyType == typeof(DateTime))
                    {
                        DateTime valueToSet = resultSet.GetDateTime(propertyName);
                        properties[i].SetValue(objectToFill, valueToSet);

                    }
                }
            }
            return (R)objectToFill;
        }

        //
        public static List<R> returnResultList(Dictionary<string, Object> dic, string procedureName)
        {
            MySqlParameter[] parameters = PrepareParameters(dic);
            List<R> results = new List<R>();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    MySqlCommand cmd = prepareCommand(procedureName, parameters);
                    conn.Open();

                    MySqlDataReader reader = cmd.ExecuteReader();

                    results = FillObjectList(reader);
                }

                return results;
            }
            catch (Exception e)
            {
                return results;
            }
        }


        public static List<R> FillObjectList(MySqlDataReader resultSet)
        {
            PropertyInfo[] properties = typeof(R).GetProperties();
            List<R> resultList = new List<R>();
            

            while (resultSet.Read())
            {
                R objectToFill = ConstructObjectToFill();

                for (int i = 0; i < properties.Length; i++)
                {
                    string propertyName = properties[i].Name;
                    Object valueToSet = new Object();

                    if (propertyName == "Message")
                    {
                        break;
                    }

                    if (properties[i].PropertyType == typeof(string))
                    {
                        valueToSet = resultSet.GetString(propertyName);
                    }

                    if (properties[i].PropertyType == typeof(int))
                    {
                        valueToSet = resultSet.GetInt32(propertyName);
                    }

                    if (properties[i].PropertyType == typeof(DateTime))
                    {
                        valueToSet = resultSet.GetDateTime(propertyName);
                    }

                    properties[i].SetValue(objectToFill, valueToSet);

                }

                resultList.Add(objectToFill);
            }
            return resultList;
        }

        public static List<Object> returnPrimitiveList(Dictionary<string, Object> dic, string procedureName)
        {
            
            MySqlParameter[] parameters = PrepareParameters(dic);
            List<Object> results = new List<Object>();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    MySqlCommand cmd = prepareCommand(procedureName, parameters);
                    conn.Open();

                    MySqlDataReader reader = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        results.Add(dt.Rows[i][0]);
                    }

                }

                return results;
            }
            catch (Exception e)
            {
                return results;
            }
        }
    }
}

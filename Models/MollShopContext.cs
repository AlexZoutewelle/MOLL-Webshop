using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MimeKit;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using TestWebApp.ElasticSearch;

namespace TestWebApp.Models
{
    
    public static class MollShopContext
    {

        public static tbl_servicedata FindServiceById(int serviceId)
        {
            tbl_servicedata foundService = new tbl_servicedata();
            Dictionary<string, Object> dic = new Dictionary<string, Object>();
            dic.Add("in_givenServiceId", serviceId);
            foundService = (tbl_servicedata)ProcedureCall<tbl_servicedata>.ExecuteReader(dic, "FindServiceById");
            return foundService;
        }

        public static tbl_labourerdata FindLabourerById(string labourerId)
        {
            tbl_labourerdata foundLabourer = new tbl_labourerdata();
            Dictionary<string, Object> dic = new Dictionary<string, Object>();
            dic.Add("in_givenLabourerId", labourerId);
            foundLabourer = ProcedureCall<tbl_labourerdata>.ExecuteReader(dic, "FindLabourerById");
            return foundLabourer;
        }

        public static tbl_userdata FindUserById(int userId)
        {
            tbl_userdata foundUser = new tbl_userdata();
            Dictionary<string, Object> dic = new Dictionary<string, Object>();
            dic.Add("in_givenUserId", userId);
            foundUser = (tbl_userdata)ProcedureCall<tbl_userdata>.ExecuteReader(dic, "FindUserById");
            return foundUser;
        }

        public static tbl_userdata FindUserByEmail(string emailAddress)
        {
            tbl_userdata foundUser = new tbl_userdata();
            Dictionary<string, Object> dic = new Dictionary<string, Object>();
            dic.Add("in_email", emailAddress);
            foundUser = (tbl_userdata)ProcedureCall<tbl_userdata>.ExecuteReader(dic, "FindUserByEmail");
            return foundUser;
        }

        public static int FindUserIdByEmail(string emailAddress)
        {
            int result = -1;
            try
            {
                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_email", emailAddress);
                dic.Add("out_userId", result);
                Dictionary<string, Object> results = ProcedureCall<int>.ExecuteNonQuery(dic, "user_FindUserIdByEmail");

                return Convert.ToInt32(results["out_userId"]);
            }
            catch (Exception e)
            {
                return result;
            }

        }

        public static int FindOrderId(int fld_offeredserviceid, string email)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, Object>();
                dic.Add("in_offeredServiceId", fld_offeredserviceid);
                dic.Add("in_givenEmail", email);
                dic.Add("out_result", -1);
                dic = ProcedureCall<int>.ExecuteNonQuery(dic, "util_getOrderId");
                return Convert.ToInt32(dic["out_result"]);
            }
            catch(Exception e)
            {
                return -1;
            }
        }

        public static int CheckIfUserNameIsTaken(string userName)
        {
            int result = -2;
            try
            {
                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_userName", userName);
                dic.Add("out_result", -2);
                dic = ProcedureCall<int>.ExecuteNonQuery(dic, "CheckIfUserNameIsTaken");
                return Convert.ToInt32(dic["out_result"]);
            }
            
            catch (Exception e)
            {
                return result;
            }
        }

        public static bool CheckShoppingCartItem(int fld_offeredserviceid, int fld_userid)
        {
            Dictionary<string, Object> dic = new Dictionary<string, Object>();
            dic.Add("in_givenServiceId", fld_offeredserviceid);
            dic.Add("in_givenUserId", fld_userid);
            dic.Add("out_result", 0);
            dic = ProcedureCall<int>.ExecuteNonQuery(dic, "CheckShoppingCartItemExistance");

            if (Convert.ToInt32(dic["out_result"]) > 0)
            {
                return true;
            }

            return false;
        }


        public static string generateActivationCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            string finalString = new String(stringChars);
            return finalString;

        }

        public static string RegisterNewUser(tbl_userdata user)
        {
            string activationCode = generateActivationCode();

            try
            {
                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_userName", user.fld_username);
                dic.Add("in_password", SaltNHash(user.fld_password));
                dic.Add("in_firstName", user.fld_firstname);
                dic.Add("in_lastName", user.fld_lastname);
                dic.Add("in_gender", user.fld_gender);
                dic.Add("in_address", user.fld_address);
                dic.Add("in_zipCode", user.fld_zipcode);
                dic.Add("in_dob", user.fld_dateofbirth);
                dic.Add("in_phoneNumber", user.fld_phonenumber);
                dic.Add("in_emailAddress", user.fld_email);
                dic.Add("in_activationCode", activationCode);
                dic.Add("in_isActivated", false);
                dic.Add("out_userId", 0);
                dic = ProcedureCall<int>.ExecuteNonQuery(dic, "user_Register");
                user.fld_activationcode = activationCode;
                user.fld_userid = Convert.ToInt32(dic["out_userId"]);
                EsUpdater<tbl_userdata>.InsertDocument(user, "moll_users", "User", dic["out_userId"].ToString());
                return activationCode;
            }
            catch (Exception e)
            {
                return "Db Error!";
            }

        }

        public static int CheckIfUserExists(string emailAddress)
        {
            int result = -1;
            try
            {
                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_emailAddress", emailAddress);
                dic.Add("out_result", result);
                dic = ProcedureCall<int>.ExecuteNonQuery(dic, "auth_CheckUserExists");
                return Convert.ToInt32(dic["out_result"]);
            }
            catch(Exception e)
            {
                return -2;
            }

        }

        public static int CheckIfPasswordsMatchs(LoginModel loginMdl)
        {
            int errorResult = 2;

            try
            {

                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_emailAddress", loginMdl.EmailAddress);
                dic.Add("in_password", SaltNHash(loginMdl.Password));
                dic.Add("out_result", -2);
                dic = ProcedureCall<int>.ExecuteNonQuery(dic, "auth_CheckUserExists");
                return Convert.ToInt32(dic["out_result"]);
            }
            catch (Exception ex)
            {
                return errorResult;
            }
        }

        public static string SaltNHash(string givenPassword)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(givenPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string passwordHash = Convert.ToBase64String(hashBytes);

            return passwordHash;
        }


        public static LoginModel UserLogin(LoginModel loginMdl)
        {
            //Check if user is found (return the password)
            Dictionary<string, Object> dic1 = new Dictionary<string, object>();
            dic1.Add("in_emailAddress", loginMdl.EmailAddress);
            tbl_userdata user = ProcedureCall<tbl_userdata>.ExecuteReader(dic1, "auth_CheckUserExistsLogin");

            string result = user.fld_password;

            if(result == null)
            {
                //Account was not found
                loginMdl.UserId = -1;
                return loginMdl;
            }


            //Check if passwords match

            //First we convert the storedPassword to bytes
            if(result != null)
            {
                string storedPassword = result.ToString();

                byte[] passwordBytes = Convert.FromBase64String(storedPassword);

                //We grab the salt
                byte[] salt = new byte[16];
                Array.Copy(passwordBytes, 0, salt, 0, 16);

                //Hash the given password and grab the resulting hash
                Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(loginMdl.Password, salt, 10000);
                byte[] givenHash = pbkdf2.GetBytes(20);

                //Compare the hashes of the stored password with the given password
                int success = 1;
                for(int i = 0; i < 20; i++)
                {
                    if(passwordBytes[i + 16] != givenHash[i])
                    {
                        loginMdl.UserId = 0;
                        return loginMdl;
                    }
                }

            }


            //Lastly, we check if the account is verified. If it is, the procedure will return all relevant information for later usage

            if(user.fld_isactivated == 0)
            {
                loginMdl.UserId = -3;
                loginMdl.UserName = user.fld_username;
                return loginMdl;
            }


            loginMdl.Admin = user.fld_adminPriv;
            loginMdl.UserName = user.fld_username;
            loginMdl.EmailAddress = user.fld_email;
            loginMdl.UserId = user.fld_userid;

            return loginMdl;
            //Retrieve relevant info
        }



        /*  
         *  CRUD Implementaties
         *  
         */
         


        public static string UpdateVarCharField(string targetTable, string fieldToChange, object valueForField, string primaryKeyColumn, int id)
        {

            string result = "";
            try
            {

                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_TargetTable", targetTable);
                dic.Add("in_FieldToChange", fieldToChange);
                dic.Add("in_valueForField", valueForField);
                dic.Add("in_PrimaryKeyColumn", primaryKeyColumn);
                dic.Add("in_Id", id);

                dic = ProcedureCall<Object>.ExecuteNonQuery(dic, "util_UpdateVarCharField");

                return "Change successful";
                
            }
            catch (Exception e)   
            {
                result = "Failed to change field";
                return result;
            }
        }

        public static string UpdateNumberField(string targetTable, string fieldToChange, int valueForField, string primaryKeyColumn, int id)
        {
            string result = "";
            try
            {
                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_TargetTable", targetTable);
                dic.Add("in_FieldToChange", fieldToChange);
                dic.Add("in_valueForField", valueForField);
                dic.Add("in_PrimaryKeyColumn", primaryKeyColumn);
                dic.Add("in_Id", id);

                dic = ProcedureCall<Object>.ExecuteNonQuery(dic, "util_UpdateNumberField");

                return "Change successful";

            }
            catch (Exception e)
            {
                result = "Failed to change field";
                return result;
            }
        }


        public static void UpdateRow(object input, string primarykeycolumn, int id)
        {

            try
            {


                string className = input.GetType().Name;
                if (!className.StartsWith("tbl_"))
                {
                    className = "tbl_" + className.ToLower();
                }
                PropertyInfo[] properties = input.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    //fieldToChange
                    string valueName = property.Name;


                    //valueForField
                    object value = property.GetValue(input);
                    if (value != null)
                    {
                        if (value.GetType() == typeof(string) || value.GetType() == typeof(char))
                        {
                            if (property.Name == "fld_password")
                            {
                                value = SaltNHash(value.ToString());
                            }

                            if (property.Name == "fld_email" && className == "tbl_userdata")
                            {
                                //We must check if the new email input is already registered or not
                                int result = CheckIfUserExists(value.ToString());
                                if (result > 0)
                                {
                                    //It already exists
                                    continue;
                                }

                            }
                            UpdateVarCharField(className, valueName, value.ToString(), primarykeycolumn, id);
                        }

                        if (value.GetType() == typeof(int))
                        {
                            if ((int)value != 0)
                            {
                                UpdateNumberField(className, valueName, (int)value, primarykeycolumn, id);
                            }
                        }

                        if (value.GetType() == typeof(DateTime))
                        {
                            UpdateVarCharField(className, valueName, ((DateTime)value).ToString("yyyy-MM-dd"), primarykeycolumn, id);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        public static string DeleteRow(string targetTable, string targetColumn, int targetValue)
        {
            string result = "";
            try
            {
                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_TargetTable", targetTable);
                dic.Add("in_TargetColumn", targetColumn);
                dic.Add("in_TargetValue",targetValue);

                dic = ProcedureCall<Object>.ExecuteNonQuery(dic, "util_DeleteRow");

                

                return "Change successful";

            }
            catch (Exception e)
            {
                result = "Failed to change field";
                return result;
            }
        }




        //Creating a row from an object

        public static int CreateRow(Object row)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("in_targetTable", row.GetType().Name);
                dic.Add("out_generatedId", -1);

                dic = CreateStrings(row, dic);
                dic = ProcedureCall<object>.ExecuteNonQuery(dic, "util_CreateRow");
                return Convert.ToInt32(dic["out_generatedId"]);

            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public static int CreateRow(Object row, string targetTable)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("in_targetTable", targetTable);
                dic.Add("out_generatedId", -1);

                dic = CreateStrings(row, dic);
                dic = ProcedureCall<object>.ExecuteNonQuery(dic, "util_CreateRow");
                return Convert.ToInt32(dic["out_generatedId"]);

            }
            catch (Exception e)
            {
                return -1;
            }
        }

        //Create input strings from an object
        public static Dictionary<string, object> CreateStrings(Object row, Dictionary<string, object> dic)
        {
            try
            {


                PropertyInfo[] properties = row.GetType().GetProperties();

                //Prepare the strings for the procedure
                string columnString = "";
                string valueString = "";

                //Loop over all properties to construct the strings
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    //Add all property names
                    columnString += properties[i].Name + ", ";

                    //Prepare the value for the column
                    object value = properties[i].GetValue(row);

                    //For the sake of SQL syntax, we need to check its type
                    if (value.GetType() == typeof(String) || value.GetType() == typeof(char) || value.GetType() == typeof(TimeSpan))
                    {
                        String realValue = value.ToString();

                        //Check for the ' character, as it will cause an SQL error upon insertion
                        realValue = realValue.Replace("'", string.Empty);

                        //Strings must be surrounded by ' '
                        valueString += "'" + realValue + "', ";
                    }

                    else if (value.GetType() == typeof(DateTime))
                    {
                        string newValue = DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd");
                        valueString += "'" + newValue + "', ";
                    }

                    else
                    {
                        //Anything else (probably) can be added as is
                        valueString += value + ", ";
                    }
                }

                //Add the last colum name
                columnString += properties[properties.Length - 1].Name;


                //Do the same for the last value
                object lastValue = properties[properties.Length - 1].GetValue(row);
                if (lastValue.GetType() == typeof(String) || lastValue.GetType() == typeof(char) || lastValue.GetType() == typeof(TimeSpan))
                {
                    valueString += "'" + properties[properties.Length - 1].GetValue(row) + "'";
                }


                else if (lastValue.GetType() == typeof(DateTime))
                {
                    string newValue = DateTime.Parse(lastValue.ToString()).ToString("yyyy-MM-dd");
                    valueString += "'" + newValue + "'";
                }

                else
                {
                    valueString += properties[properties.Length - 1].GetValue(row);
                }


                //Add the two strings to our dictionary
                dic.Add("in_ColumnString", columnString);
                dic.Add("in_ValueString", valueString);

                return dic;
            }
            catch(Exception e)
            {
                return dic;
            }
        }



        //Creating a row using a ModelStateDictionary
        public static int CreateRow(ModelStateDictionary modelState, string targetTable)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("in_TargetTable", targetTable);
                dic.Add("out_generatedId", -1);

                dic = CreateStrings(modelState, dic);
                dic = ProcedureCall<object>.ExecuteNonQuery(dic, "util_CreateRow");
                return Convert.ToInt32(dic["out_generatedId"]);

            }
            catch (Exception e)
            {
                return -1;
            }
        }

        //Create string for procedure from a ModelStateDictionary
        public static Dictionary<string,object> CreateStrings(ModelStateDictionary modelState, Dictionary<string, object> dic)
        {
            try
            {
                //Pak de namen van de parameters
                List<string> keys = modelState.Keys.ToList<string>();

                //Pak de waarden van de parameters
                List<object> values = (modelState.Values.Select(s => s.RawValue)).ToList<object>();

                return ConstructStrings(dic, keys, values);
            }

            catch(Exception e)
            {
                throw e;
            }
        }

        //The actual creation of the Strings for the procedure
        public static Dictionary<string,object> ConstructStrings(Dictionary<string, object> dic, List<string> keys, List<object> values)
        {
            //Maak van de keys een string
            string in_ColumnString = "";
            for (int i = 0; i < keys.Count - 1; i++)
            {
                in_ColumnString += keys[i] + ", ";
            }
            //Voeg de laatste eraan toe
            in_ColumnString += keys[keys.Count - 1];

            //Maak nu van de values een string
            string in_ValueString = "";
            for (int i = 0; i < values.Count - 1; i++)
            {
                if (values[i].GetType() == typeof(string))
                {
                    in_ValueString += "'" + values[i] + "', ";
                }

                else
                {
                    in_ValueString += values[i] + ", ";
                }
            }
            //Voeg de laatste eraan toe
            if (values[values.Count - 1].GetType() == typeof(string))
            {
                in_ValueString += "'" + values[values.Count - 1] + "', ";
            }

            else
            {
                in_ValueString += values[values.Count - 1] + ", ";
            }

            //Stop ze in de parameter dictionary
            dic.Add("in_ColumnString", in_ColumnString);
            dic.Add("in_ValueString", in_ValueString);

            //Return de dictionary
            return dic;
        }


        public static List<Object> GetShoppingCartItems(int userid)
        {
            Dictionary<string, object> dic = new Dictionary<string, Object>();
            dic.Add("in_givenUserId", userid);
            List<Object> offeredServiceIds = ProcedureCall<Object>.returnPrimitiveList(dic, "GetShoppingCartItems");


            return offeredServiceIds;
        }

        public static void SendEmail(string emailAddress, string subject, string messageBody)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(subject, "MollWebShop@gmail.com"));

                message.To.Add(new MailboxAddress(emailAddress));

                BodyBuilder builder = new BodyBuilder();
                builder.HtmlBody = messageBody;


                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("MollWebShop@gmail.com", "MollWachtwoord1!");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception e)
            {

            }

        }

        public static int VerifyUser(string token, int userid)
        {
            int result = 0;
            try
            {
                Dictionary<string, Object> dic = new Dictionary<string, Object>();
                dic.Add("in_token", token);
                dic.Add("in_userid", userid);
                dic.Add("out_result", result);

                dic = ProcedureCall<int>.ExecuteNonQuery(dic, "user_verify");

                EsUpdater<tbl_userdata>.UpdateField("" + userid, "fld_isactivated", 1);

                return Convert.ToInt32(dic["out_result"]);
            }
            catch(Exception e)
            {
                return result;
            }
        }

    }
}
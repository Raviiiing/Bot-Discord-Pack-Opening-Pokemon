using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Bot_Discord_Jeu_de_Carte.Modules
{
    class Bdd
    {

        private MySqlConnection connection;

        // Constructeur
        public Bdd()
        {
            this.InitConnexion();
        }

        // Méthode pour initialiser la connexion
        private void InitConnexion()
        {
            // Création de la chaîne de connexion
            string connectionString = "SERVER=ip; DATABASE=nom_de_base; UID=username; PASSWORD=password!";
            this.connection = new MySqlConnection(connectionString);
        }

        // Méthode pour ajouter un contact
        public void AddUser(ulong id, string username)
        {
            try
            {
                this.connection.Open();
                MySqlCommand cmd = this.connection.CreateCommand();

                cmd.CommandText = "INSERT INTO user_table (user_id, username, lastTime) VALUES (@user_id, @username, @time)";

                string dateTime = DateTime.Now.ToString();
                string createddate = Convert.ToDateTime(dateTime).ToString("yyyy-MM-dd h:mm tt");
                DateTime dt = DateTime.ParseExact(createddate, "yyyy-MM-dd h:mm tt", CultureInfo.InvariantCulture);

                cmd.Parameters.AddWithValue("@user_id", id);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@time", dt);

                cmd.ExecuteNonQuery();

                this.connection.Close();
            }
            catch
            {

            }
        }

        public void AddCardToUser(string num_card, ulong id, string table)
        {
            try
            {
                this.connection.Open();
                MySqlCommand cmd = this.connection.CreateCommand();

                cmd.CommandText = "INSERT INTO " + table + " (user_id, pkm_card, nbr_card) VALUES (@user_id, @pkm_card, @nbr_card)";

                cmd.Parameters.AddWithValue("@user_id", id);
                cmd.Parameters.AddWithValue("@pkm_card", num_card);
                cmd.Parameters.AddWithValue("@nbr_card", MySqlDbType.Int64).Value = 1;


                cmd.ExecuteNonQuery();

                Console.WriteLine("{0} : Carte sauvegarder", num_card);
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool IsCardInTable(string num_card, ulong id, string table)
        {
            try
            {
                Bdd bdd = new Bdd();
                string result = null;
                string query = "SELECT pkm_card FROM " + table + " WHERE user_id = " + id + " AND pkm_card = " + num_card;
                this.connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, this.connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = dataReader.GetString(0);
                    }
                }
                this.connection.Close();
                if (result is not null)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public string SelectCard(string num_card, ulong id, string table)
        {
            try
            {
                Bdd bdd = new Bdd();
                string result = null;
                string query = "SELECT nbr_card FROM " + table + " WHERE user_id = " + id + " AND pkm_card = " + num_card;
                this.connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, this.connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = dataReader.GetString(0);
                    }
                }
                this.connection.Close();
                if (result is not null)
                    return result;
                else
                    return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public void UpdateList(string num_card, ulong id, string table, string nb_card)
        {
            try
            {
                this.connection.Open();
                MySqlCommand cmd = this.connection.CreateCommand();
                int nb = Convert.ToInt32(nb_card) + 1;

                cmd.CommandText = "UPDATE " + table + " SET nbr_card=" + nb + " WHERE user_id=" + id + " AND pkm_card=" + num_card;

                cmd.ExecuteNonQuery();

                Console.WriteLine("{0} : Carte double ajoutée", num_card);
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public bool Select(ulong id)
        {
            try
            {
                Bdd bdd = new Bdd();
                string result = null;
                string query = "SELECT username FROM user_table WHERE user_id = " + id;

                this.connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, this.connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = dataReader.GetString(0);
                    }
                }
                this.connection.Close();
                if (result is not null)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public int SelectCountCard(ulong id, string table)
        {
            int result = 0;
            try
            {
                Bdd bdd = new Bdd();
                string query = "SELECT count(id) FROM " + table + " WHERE user_id = " + id;

                this.connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, this.connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = Convert.ToInt32(dataReader.GetString(0));
                    }
                }
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }

        public string SelectDateTime(ulong id)
        {

            try
            {
                Bdd bdd = new Bdd();
                string result = null;
                string query = "SELECT lastTime FROM user_table WHERE user_id = " + id;

                this.connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, this.connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = dataReader.GetString(0);
                    }
                }
                this.connection.Close();
                if (result is not null)
                    return result;
                else
                    return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void UpdateLastTime(ulong id, DateTimeOffset last)
        {
            try
            {
                this.connection.Open();
                MySqlCommand cmd = this.connection.CreateCommand();

                DateTime targetTime;

                targetTime = last.DateTime;

                targetTime.ToString("yyyy-MM-dd h:mm tt");

                cmd.CommandText = "UPDATE user_table SET lastTime = @time WHERE user_id = @id ";
                cmd.Parameters.AddWithValue("@time", targetTime);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                //Console.WriteLine("{0} LastTime Updated: {1}", id, targetTime);
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public List<string> SelectAll()
        {
            try
            {
                Bdd bdd = new Bdd();

                string query = "SELECT * FROM  user_table;";
                List<string> results = new List<string>();

                this.connection.Open();

                MySqlCommand cmd = new MySqlCommand(query, this.connection);

                MySqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string row = dataReader.GetString(1) + " " + dataReader.GetString(2);
                        Console.WriteLine("Ligne ajoutée : {0}", row);
                        results.Add(row);
                    }
                }
                this.connection.Close();
                if (results != null)
                    return results;
                else
                    return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public int SelectMoney(ulong id)
        {
            int result = 0;
            try
            {
                Bdd bdd = new Bdd();

                string query = "SELECT money FROM  user_table WHERE user_id = " + id;

                this.connection.Open();

                MySqlCommand cmd = new MySqlCommand(query, this.connection);

                MySqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = Convert.ToInt32(dataReader.GetString(0));
                    }
                }
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }

        public void AddMoney(ulong id, int amount, int pastAmount)
        {
            try
            {
                this.connection.Open();
                MySqlCommand cmd = this.connection.CreateCommand();

                amount += pastAmount;
                cmd.CommandText = "UPDATE user_table SET money=" + amount + " WHERE user_id=" + id;

                cmd.ExecuteNonQuery();

                Console.WriteLine("Argent actualisé Solde + {0} ", amount);
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void RemoveMoney(ulong id, int amount, int pastAmount)
        {
            try
            {
                this.connection.Open();
                MySqlCommand cmd = this.connection.CreateCommand();

                amount = pastAmount - amount;

                cmd.CommandText = "UPDATE user_table SET money=" + amount + " WHERE user_id=" + id;

                cmd.ExecuteNonQuery();

                Console.WriteLine("Argent actualisé Solde - {0} ", amount);
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public int SelectMacroBooster(ulong id)
        {
            int result = 0;
            try
            {
                Bdd bdd = new Bdd();

                string query = "SELECT macroBooster FROM  user_table WHERE user_id = " + id;
                this.connection.Open();

                MySqlCommand cmd = new MySqlCommand(query, this.connection);

                MySqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = Convert.ToInt32(dataReader.GetString(0));
                    }
                }
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }

        public void UpdateMacroBooster(ulong id, int NewAmount)
        {
            try
            {
                this.connection.Open();
                MySqlCommand cmd = this.connection.CreateCommand();

                cmd.CommandText = "UPDATE user_table SET macroBooster=" + NewAmount + " WHERE user_id=" + id;

                cmd.ExecuteNonQuery();
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public int SelectDuplicateCount(ulong id, string table)
        {
            int result = 0;
            try
            {
                Bdd bdd = new Bdd();
                string query = "SELECT sum(nbr_card) FROM " + table + " WHERE user_id = " + id;
                Console.WriteLine("ici");
                this.connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, this.connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result = Convert.ToInt32(dataReader.GetString(0));
                    }
                }
                this.connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }
    }
}

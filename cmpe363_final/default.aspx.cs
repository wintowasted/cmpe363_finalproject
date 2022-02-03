using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Azure.AI.TextAnalytics;
using DetectedLanguage = Azure.AI.TextAnalytics.DetectedLanguage;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Configuration; // Namespace for ConfigurationManager
using System.Threading.Tasks; // Namespace for Task
using Azure.Identity;
using Azure.Storage.Queues; // Namespace for Queue storage types
using Azure.Storage.Queues.Models; // Namespace for PeekedMessage

namespace cmpe363_final
{

    public partial class _default : System.Web.UI.Page
    {
        private static AzureKeyCredential credentials; 
        private static  Uri endpoint;
        private static TextAnalyticsClient client;
        SqlConnectionStringBuilder builder;
        public static int queue_id = 1;
        public static string queue_name = "queue";
        public static string connection_string = "";

        protected void Page_Load(object sender, EventArgs e)
        {



            builder = new SqlConnectionStringBuilder();
            builder.DataSource = "cmpe363finaldatabase.database.windows.net";
            builder.UserID = "saadmin";
            builder.Password = "Asadmin321";
            builder.InitialCatalog = "cmpe363db";

            String sql_table = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='History' and xtype='U') CREATE TABLE History(text varchar(100), result varchar(20)) ";

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sql_table, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }
            }
        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {
           
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            DetectedLanguage detectedLanguage = client.DetectLanguage(TextBox1.Text);
            TextBox2.Text = detectedLanguage.Name;

            CreateQueueClient(queue_name);
            CreateQueue(queue_name);
            InsertMessage(queue_name, TextBox1.Text);
            PeekMessage(queue_name);

            try
            {
                
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {

                   
                    String sql = "Insert into History(text,result) values(" + "'" + TextBox1.Text + "'" + "," + "'" + TextBox2.Text + "'" + ")";

                  

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                    }


                }


            }
            catch (SqlException error)
            {
                TextBox2.Text = error.Message;
            }

            GridviewBind();

        }

        protected void GridviewBind()
        {
           
        
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("Select * FROM History", connection);
                SqlDataReader dr = cmd.ExecuteReader();
                GridView1.DataSource = dr;
                GridView1.DataBind();
                connection.Close();
            }
        }

        public void CreateQueueClient(string queueName)
        {
            // Get the connection string from app settings
            string connectionString = connection_string;

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName);
        }

        //-------------------------------------------------
        // Create a message queue
        //-------------------------------------------------
        public bool CreateQueue(string queueName)
        {
            try
            {
                // Get the connection string from app settings
                string connectionString = connection_string;

                // Instantiate a QueueClient which will be used to create and manipulate the queue
                QueueClient queueClient = new QueueClient(connectionString, queueName);

                // Create the queue
                queueClient.CreateIfNotExists();

                if (queueClient.Exists())
                {
                    //Console.WriteLine($"Queue created: '{queueClient.Name}'");
                    return true;
                }
                else
                {
                    TextBox2.Text = "Make sure the Azurite storage emulator running and try again.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                TextBox2.Text = ex.Message;
                return false;
            }
        }

        //-------------------------------------------------
        // Insert a message into a queue
        //-------------------------------------------------
        public void InsertMessage(string queueName, string message)
        {
            // Get the connection string from app settings
            string connectionString = connection_string;

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            // Create the queue if it doesn't already exist
            queueClient.CreateIfNotExists();

            if (queueClient.Exists())
            {
                // Send a message to the queue
                queueClient.SendMessage(message);
            }

            Label3.Text = $"Last search : {message}";
        }

        //-------------------------------------------------
        // Peek at a message in the queue
        //-------------------------------------------------
        public void PeekMessage(string queueName)
        {
            // Get the connection string from app settings
            string connectionString = connection_string;

            // Instantiate a QueueClient which will be used to manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            if (queueClient.Exists())
            {
                // Peek at the next message
                PeekedMessage[] peekedMessage = queueClient.PeekMessages();

                // Display the message
                Label2.Text = $"First search: '{peekedMessage[0].Body}'";
            }
        }

        public void DeleteQueue(string queueName)
        {
            // Get the connection string from app settings
            string connectionString = connection_string;

            // Instantiate a QueueClient which will be used to manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            if (queueClient.Exists())
            {
                // Delete the queue
                queueClient.Delete();
            }


        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            DeleteQueue(queue_name);
            queue_name += queue_id.ToString();
            queue_id++;
            Label3.Text = "";
            Label2.Text = "";
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            credentials = new AzureKeyCredential(TextBox4.Text);
            endpoint = new Uri(TextBox5.Text);
            client = new TextAnalyticsClient(endpoint, credentials);
            connection_string = TextBox3.Text;

        }

        protected void TextBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
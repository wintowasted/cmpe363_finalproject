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


namespace cmpe363_final
{

    public partial class _default : System.Web.UI.Page
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("1c5529bbead546e5a70b89e0d6d7ee2b");
        private static readonly Uri endpoint = new Uri("https://cmpe363languageai.cognitiveservices.azure.com/");
        TextAnalyticsClient client;
        
       

        protected void Page_Load(object sender, EventArgs e)
        {
            client = new TextAnalyticsClient(endpoint, credentials);
 
        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {
           
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            DetectedLanguage detectedLanguage = client.DetectLanguage(TextBox1.Text);
            TextBox2.Text = detectedLanguage.Name;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "cmpe363final.database.windows.net";
                builder.UserID = "saadmin";
                builder.Password = "Asadmin123";
                builder.InitialCatalog = "final_db";

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

           

        }
    }

  
}
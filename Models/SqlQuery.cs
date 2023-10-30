using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ReportServices.Models
{
    public class SqlQuery
    {
          public static string getProductCategory()
        {

            using (SqlConnection connection = new SqlConnection("Data Source=dataplatformdemodata.syncfusion.com;Initial Catalog=AdventureWorks2016;user id=demoreadonly@data-platform-demo;password=N@c)=Y8s*1&dh;"))
            {
                connection.Open();

                string queryString = "SELECT DISTINCT ProductCategoryID, Name FROM Production.ProductCategory";
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);

                using (DataSet ProductCategories = new DataSet())
                {
                    adapter.Fill(ProductCategories, "Orders");
                    HttpContext.Current.Cache.Insert("ProductCategoryDetail", ProductCategories.Tables[0]);
                    connection.Close();
                    return JsonConvert.SerializeObject(HttpContext.Current.Cache.Get("ProductCategoryDetail"));
                }
            }
        }

        public static string getProductSubCategory()
        {
            using (SqlConnection connection = new SqlConnection("Data Source=dataplatformdemodata.syncfusion.com;Initial Catalog=AdventureWorks2016;user id=demoreadonly@data-platform-demo;password=N@c)=Y8s*1&dh;"))
            {
                connection.Open();
                string queryString = $"SELECT ProductSubcategoryID, ProductCategoryID, Name FROM Production.ProductSubcategory ";
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);

                using (DataSet ProductSubCategories = new DataSet())
                {
                    adapter.Fill(ProductSubCategories);
                    HttpContext.Current.Cache.Insert("ProductSubCategoryDetail", ProductSubCategories.Tables[0]);
                    connection.Close();
                    return JsonConvert.SerializeObject(HttpContext.Current.Cache.Get("ProductSubCategoryDetail"));
                }
            }
        }
    }
}
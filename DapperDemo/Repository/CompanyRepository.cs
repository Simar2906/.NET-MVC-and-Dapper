using System.Data;
using System.Runtime.Intrinsics.Arm;
using Dapper;
using DapperDemo.Data;
using DapperDemo.Models;
using Microsoft.Data.SqlClient;

namespace DapperDemo.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private IDbConnection db;

        public CompanyRepository(IConfiguration configuration)
        {
            this.db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        public Company Add(Company company)
        {
            var sql = "INSERT INTO Companies(Name, Address, City, State, PostalCode) VALUES (@Name, @Address, @City, @State, @PostalCode); SELECT CAST(SCOPE_IDENTITY() as int);";
            var id = db.Query<int>(sql, company/*new { @name = company.Name, @add = company.Address, @city = company.City, @state = company.State, @postal = company.PostalCode }*/).Single();
            company.CompanyId = id;
            Console.WriteLine("aoow");
            return company;
        }

        public Company Find(int id)
        {
            var sql = $"SELECT * FROM Companies WHERE CompanyId= @CompanyID";
            return db.Query<Company>(sql, new {@CompanyID = id}).Single();
        }

        public List<Company> GetAll()
        {
            var sql = "SELECT * FROM Companies";
            return db.Query<Company>(sql).ToList();
        }

        public void Remove(int id)
        {
            var sql = "DELETE FROM Companies WHERE CompanyId = @id";
            db.Execute(sql, new {id});
        }

        public Company Update(Company company)
        {
            var sql = "UPDATE Companies SET Name = @Name, Address = @Address, City = @City, State = @State, PostalCode = @PostalCode WHERE CompanyId = @CompanyId";

            db.Execute(sql, company);
            return company;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Serialization;
using QMap.Configuration;
using Dapper;
using System.Configuration;
using System.IO;

namespace QMap
{
    [XmlType("requests")]
    public class DbContext : IDbContext
    {
        #region static members

        private const string NoRequestFoundMessage = "Unable to find request : {0} in the requests list";

        private static DbContext _requestRepository;

        public static DbContext Instance
        {
            get
            {
                if (_requestRepository == null)
                {
                    _requestRepository = LoadRequestRepo();
                    SetTypeMaps();

                }
                return _requestRepository;
            }
        }


        private static DbContext LoadRequestRepo()
        {

            string requestRepositoryFileName = ConfigurationManager.AppSettings["marketdataRequestsFile"] ?? "dataconfig.xml";

            if (requestRepositoryFileName == null)
            {
                throw new ApplicationException("Please provided a 'requestRepositoryFile' entry under appSettings");
            }
            DbContext requestRepo = null;
            var serialiser = new XmlSerializer(typeof(DbContext));
            using (var stream = new StreamReader(GetAbsolutePath(requestRepositoryFileName)))
            {
                requestRepo = (DbContext)serialiser.Deserialize(stream);
            }
            return requestRepo;
        }




        #endregion


        #region .ctor
        private DbContext()
        {
        }
        #endregion


        #region instance fields
        [XmlElement("request")]
        public List<Request> Requests { get; set; }


        #endregion


        #region public methods
        public IPropertyCollection Execute(IDbConnectionProvider connectionProvider, string requestName, IDictionary<string, object> parameters)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException("connectionProvider");
            var request = FindRequest(requestName);
            if (request == null)
                throw new ApplicationException(string.Format(NoRequestFoundMessage, requestName));

            using (var dbconnection = connectionProvider.GetConnection(request.ConnectionName))
            {
                var dynamicParameters = PrepareParameters(request, parameters);
                dbconnection.Execute(sql: request.Text, param: dynamicParameters, commandType: request.CommandType);
                return FillParameters(dynamicParameters);
            }

        }



        /*

        public Task<IPropertyCollection> ExecuteAsync(IDbConnectionProvider connectionProvider, string requestName, IDictionary<string, object> parameters)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException("connectionProvider");
            var request = FindRequest(requestName);
            if (request == null)
                throw new ApplicationException(string.Format(NoRequestFoundMessage, requestName));
            var dbconnection = connectionProvider.GetConnection(request.ConnectionName);

            return PrepareCommand(dbconnection, request, parameters);


        }

        private Task<IPropertyCollection> PrepareCommand(System.Data.IDbConnection dbconnection, Request request, IDictionary<string, object> parameters)
        {
            if (!TryOpenConnection(dbconnection))
            {
               // Console.WriteLine("Connection cannot be opened anymore!");
                return Task.FromResult(NullMarketDataResponse.Value as IPropertyCollection);
            }
            var command = dbconnection.CreateCommand();
            command.CommandText = request.Text;
            command.CommandType = request.CommandType;
            foreach (var param in request.Parameters)
            {
                object objectVal = parameters.ContainsKey(param.Name) ? parameters[param.Name] : DBNull.Value;
                var dbParam = command.CreateParameter();
                dbParam.ParameterName = param.Name;
                dbParam.Value = objectVal;
                dbParam.DbType = param.ParameterType;
                dbParam.Direction = param.Direction;
                dbParam.Size = param.Size;
                command.Parameters.Add(dbParam);
            }
            if (command is AseCommand)
                return RunAseCommandAsynchronously((AseCommand)command);
            if (command is SqlCommand)
                return RunSqlCommandAsynchronously((SqlCommand)command);
            throw new NotSupportedException(String.Format("Asynchronous calls for IO completion port not supported driver from {0}", request.ConnectionName));
        }
         * 
         *  private Task<IPropertyCollection> RunAseCommandAsynchronously(AseCommand command)
        {
            return Task.Factory.FromAsync<int>(command.BeginExecuteNonQuery, command.EndExecuteNonQuery, null)
           .ContinueWith(r =>
                             {
                                 var output = new Dictionary<string, object>();
                                 foreach (var parameter in command.Parameters)
                                 {
                                     var aseParameter = (AseParameter)parameter;
                                     output[aseParameter.ParameterName] = aseParameter.Value;
                                 }
                                 var connection = command.Connection;
                                 command.Dispose();
                                 connection.Dispose();
                                 IPropertyCollection outputParameters = new CommonParameterCollection(output);
                                 return outputParameters;

                             });
        }
         * */

        private bool TryOpenConnection(System.Data.IDbConnection dbconnection)
        {
            int max_atempts = 3;
            for (int i = 0; i < max_atempts; i++)
            {
                try
                {
                    dbconnection.Open();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return false;
        }

        private Task<IPropertyCollection> RunSqlCommandAsynchronously(SqlCommand command)
        {
            throw new NotSupportedException("Asynchronous calls for IO completion port not supported drive");

        }

       





        public IEnumerable<T> Query<T>(IDbConnectionProvider connectionProvider, string requestName, IDictionary<string, object> parameters)
        {
            if (connectionProvider == null)
                throw new ArgumentNullException("connectionProvider");
            var request = FindRequest(requestName);
            if (request == null)
                throw new ApplicationException(string.Format(NoRequestFoundMessage, requestName));
            using (var dbconnection = connectionProvider.GetConnection(request.ConnectionName))
            {
                var dynamicParameters = PrepareParameters(request, parameters);
                return dbconnection.Query<T>(sql: request.Text, param: dynamicParameters, commandType: request.CommandType);

            }
        }

        #endregion

        #region private methods

        private IPropertyCollection FillParameters(DynamicParameters dynamicParameters)
        {
            return new DynamicParametersCollection(dynamicParameters);

        }


        private Request FindRequest(string requestName)
        {
            return Requests.FirstOrDefault(r => r.RequestName == requestName);
        }



        private DynamicParameters PrepareParameters(Request request, IDictionary<string, object> parameters)
        {

            var dynamicParameters = new DynamicParameters();

            foreach (var parameter in request.Parameters)
            {
                object objectVal = (parameters != null && parameters.ContainsKey(parameter.Name)) ? parameters[parameter.Name] : null;
                dynamicParameters.Add(parameter.Name, value: objectVal, dbType: parameter.ParameterType, direction: parameter.Direction, size: parameter.Size);
            }


            return dynamicParameters;
        }

        private static string GetAbsolutePath(string relativePath)
        {
            string absoluteFileLocation =
               Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase),
               relativePath);
            if (absoluteFileLocation.StartsWith("file:\\")) absoluteFileLocation = absoluteFileLocation.Remove(0, 6);

            return Path.GetFullPath(absoluteFileLocation);
        }

        private static void SetTypeMaps()
        {
            var types = typeof(IEntity).Assembly.GetTypes().Where(t => typeof(IEntity).IsAssignableFrom(t) && !t.IsInterface);

            foreach (Type t in types)
            {
                SqlMapper.SetTypeMap(
                    t,
                    new CustomPropertyTypeMap(
                        t,
                        (type, columnName) => type
                                                  .GetProperties()
                                                  .FirstOrDefault(prop => prop
                                                                              .GetCustomAttributes(false)
                                                                              .OfType<ColumnAttribute>()
                                                                              .Any(attr => string.Equals(attr.Name, columnName, StringComparison.OrdinalIgnoreCase)))));
            }
        }

        #endregion


    }
}

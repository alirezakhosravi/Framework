using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data.Extensions;
using Nop.Data.Mapping;

namespace Nop.Data
{
    /// <summary>
    /// Represents base object context
    /// </summary>
    public partial class NopObjectContext : DbContext, IDbContext, IConfigurationDbContext
    {
        #region Ctor

        public NopObjectContext(DbContextOptions<NopObjectContext> options) : base(options)
        {
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Further configuration the model
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //dynamically load all entity and query type configurations
            var typeConfigurations = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                (type.BaseType?.IsGenericType ?? false)
                    && (type.BaseType.GetGenericTypeDefinition() == typeof(NopEntityTypeConfiguration<>)));

            foreach (var typeConfiguration in typeConfigurations)
            {
                var configuration = (IMappingConfiguration)Activator.CreateInstance(typeConfiguration);
                configuration.ApplyConfiguration(modelBuilder);
            }

            //this.CheckConflict();

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Modify the input SQL query by adding passed parameters
        /// </summary>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>Modified raw SQL query</returns>
        protected virtual string CreateSqlWithParameters(string sql, params object[] parameters)
        {
            //add parameters to sql
            for (var i = 0; i <= (parameters?.Length ?? 0) - 1; i++)
            {
                if (!(parameters[i] is DbParameter parameter))
                    continue;

                sql = $"{sql}{(i > 0 ? "," : string.Empty)} @{parameter.ParameterName}";

                //whether parameter is output
                if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                    sql = $"{sql} output";
            }

            return sql;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a DbSet that can be used to query and save instances of entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A set for the given entity type</returns>
        public virtual new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// Generate a script to create all tables for the current model
        /// </summary>
        /// <returns>A SQL script</returns>
        public virtual string GenerateCreateScript()
        {
            return this.Database.GenerateCreateScript();
        }

        /// <summary>
        /// Creates a LINQ query for the query type based on a raw SQL query
        /// </summary>
        /// <typeparam name="TQuery">Query type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public virtual IQueryable<TQuery> QueryFromSql<TQuery>(string sql) where TQuery : class
        {
            return base.Set<TQuery>().FromSqlRaw(sql);
        }

        /// <summary>
        /// Creates a LINQ query for the entity based on a raw SQL query
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <param name="parameters">The values to be assigned to parameters</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public virtual IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity
        {
            return this.Set<TEntity>().FromSqlRaw(CreateSqlWithParameters(sql, parameters), parameters);
        }

        /// <summary>
        /// Executes the given SQL against the database
        /// </summary>
        /// <param name="sql">The SQL to execute</param>
        /// <param name="doNotEnsureTransaction">true - the transaction creation is not ensured; false - the transaction creation is ensured.</param>
        /// <param name="timeout">The timeout to use for command. Note that the command timeout is distinct from the connection timeout, which is commonly set on the database connection string</param>
        /// <param name="parameters">Parameters to use with the SQL</param>
        /// <returns>The number of rows affected</returns>
        public virtual int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            //set specific command timeout
            var previousTimeout = this.Database.GetCommandTimeout();
            this.Database.SetCommandTimeout(timeout);

            var result = 0;
            if (!doNotEnsureTransaction)
            {
                //use with transaction
                using (var transaction = this.Database.BeginTransaction())
                {
                    result = this.Database.ExecuteSqlRaw(sql, parameters);
                    transaction.Commit();
                }
            }
            else
                result = this.Database.ExecuteSqlRaw(sql, parameters);

            //return previous timeout back
            this.Database.SetCommandTimeout(previousTimeout);

            return result;
        }

        /// <summary>
        /// Detach an entity from the context
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        public virtual void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityEntry = this.Entry(entity);
            if (entityEntry == null)
                return;

            //set the entity is not being tracked by the context
            entityEntry.State = EntityState.Detached;
        }

        public string GetTableNameByType(Type type, bool sqlType = false)
        {
            var tableName = base.Model.FindEntityType(type).GetTableName();
            var schema = base.Model.FindEntityType(type).GetSchema();

            return sqlType ? $"[{schema}].[{tableName}]" : $"{schema}.{tableName}";
        }

        /// <summary>
        /// Creates a LINQ query for the query type based on a raw SQL query
        /// </summary>
        /// <typeparam name="TQuery">Query type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public IList<TQuery> DynamicSqlQuery<TQuery>(string sql, CommandType commandType, params DbParameter[] parameters) where TQuery : class
        {
            using (var command = base.Database.GetDbConnection().CreateCommand())
            {
                try
                {
                    base.Database.OpenConnection();
                    command.CommandText = sql;
                    command.CommandType = commandType;

                    foreach (var param in parameters)
                    {
                        DbParameter dbParameter = command.CreateParameter();
                        dbParameter.DbType = param.DbType;
                        dbParameter.ParameterName = param.ParameterName;
                        dbParameter.Value = param.Value;
                        command.Parameters.Add(dbParameter);
                    }

                    var result = command.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(result);
                    var results = dataTable.ToListof<TQuery>();

                    return (results);
                }
                finally
                {
                    base.Database.CloseConnection();
                }
            }
        }

        /// <summary>
        /// Update the database.
        /// </summary>
        public void UpdateDatabase()
        {
            try
            {
                this.Database.Migrate();
            }
            catch
            {
                // do noting
            }
        }

        /// <summary>
        /// create temporal table
        /// </summary>
        public void AddTemporal()
        {
            string createSchema = $@"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'History')
                            BEGIN
                                EXEC('CREATE SCHEMA History')
                            END";
            ExecuteSqlCommand(createSchema);
            
            ITypeFinder _typeFinder = EngineContext.Current.Resolve<ITypeFinder>();
            var listOfTempralClass = _typeFinder.FindClassesOfType<ITemporal>(_typeFinder.GetAssemblies().Where(e => e.GetName().ToString().ToLower().Contains("nop.core"))).ToList();
            
            foreach (var item in listOfTempralClass)
            {
                string query = $@"IF NOT EXISTS(SELECT * FROM {this.GetTableNameByType(item, true)}) BEGIN
                                IF NOT EXISTS(SELECT * FROM   INFORMATION_SCHEMA.COLUMNS WHERE  CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) = '{this.GetTableNameByType(item)}' AND COLUMN_NAME = 'SysStartTime')
                                BEGIN
	                                ALTER TABLE {this.GetTableNameByType(item, true)} ADD 
                                        SysStartTime datetime2(0) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
                                        SysEndTime datetime2(0) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
                                        PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
                                END
                            END";

                ExecuteSqlCommand(query);

                query = $@"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'History' AND  TABLE_NAME = '{this.GetTableNameByType(item).Split('.')[1]}') ALTER TABLE {this.GetTableNameByType(item, true)} SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = history.{this.GetTableNameByType(item, true).Split('.')[1]}));";

                ExecuteSqlCommand(query);
            }
        }

        /// <summary>
        /// create change traking
        /// </summary>
        public void AddChangeTracking(NopConfig config)
        {
            if (!config.EnableChangeTracking)
            {
                return;
            }

            string query = $@"IF NOT EXISTS (SELECT * FROM sys.change_tracking_databases WHERE database_id=DB_ID(DB_NAME())) BEGIN
	                            ALTER DATABASE CURRENT SET CHANGE_TRACKING = ON (CHANGE_RETENTION = {config.ChangeRetention} DAYS, AUTO_CLEANUP = ON)
                            END";
            ExecuteSqlCommand(query, doNotEnsureTransaction: true);

            ITypeFinder _typeFinder = EngineContext.Current.Resolve<ITypeFinder>();
            var listOfTempralClass = _typeFinder.FindClassesOfType<IChangeTracking>(_typeFinder.GetAssemblies().Where(e => e.GetName().ToString().ToLower().Contains("nop.core"))).ToList();

            foreach (var item in listOfTempralClass)
            {
                query = $@"IF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('{this.GetTableNameByType(item)}')) ALTER TABLE {this.GetTableNameByType(item, true)} ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)";
                ExecuteSqlCommand(query);
            }
        }

        /// <summary>
        /// check conflict between change traking and temporal table
        /// </summary>
        public void CheckConflict()
        {
            ITypeFinder _typeFinder = EngineContext.Current.Resolve<ITypeFinder>();
            var listOfChangeTraking = _typeFinder.FindClassesOfType<IChangeTracking>(_typeFinder.GetAssemblies().Where(e => e.GetName().ToString().ToLower().Contains("nop.core"))).ToList();

            foreach (var item in listOfChangeTraking)
            {
                if (item.GetInterface(nameof(ITemporal)) != null)
                {
                    throw new Exception($"The {item.Name} in {item.Namespace} namespace has both of interface ITemporal and IChangeTracking");
                }
            }
        }
        #endregion
    }
}
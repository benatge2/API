using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient; // Librería correcta para MySQL
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Odoo_iController : ControllerBase
    {
        // Credenciales de la base de datos
        private readonly string databaseIp = "192.168.115.153";
        private readonly string databaseName = "4_erronka1";
        private readonly string databaseUser = "benat";
        private readonly string databasePassword = "1WMG2023";
        private readonly string port = "3306";

        //using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
        // Método para obtener la conexión a la base de datos
        private MySqlConnection GetDbConnection()
        {
            string connectionString = $"Server={databaseIp};Port={port};Database={databaseName};User={databaseUser};Password={databasePassword};";
            return new MySqlConnection(connectionString);
        }

        [HttpGet("GetEgunerokoBezeroak")]
        public async Task<IActionResult> GetEgunerokoBezeroak()
        {
            try
            {
                var result = new List<object>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 
                            DAYNAME(ep.eskaera_ordua) AS dia_semana,
                            WEEK(ep.eskaera_ordua) AS semana_ano,
                            YEAR(ep.eskaera_ordua) AS ano,
                            COUNT(DISTINCT ep.eskaera_id) AS eskaera_kopurua,
                            COALESCE(SUM(p.prezioa), 0) AS fakturazioa_total
                        FROM 
                            eskaera_platera ep
                        JOIN 
                            platera p ON ep.platera_id = p.id
                        GROUP BY 
                            YEAR(ep.eskaera_ordua), 
                            WEEK(ep.eskaera_ordua), 
                            DAYNAME(ep.eskaera_ordua)
                        ORDER BY 
                            ano DESC,
                            semana_ano DESC, 
                            FIELD(dia_semana, 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday');";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new
                            {
                                dia_semana = reader["dia_semana"].ToString(),
                                semana_ano = reader.GetInt32("semana_ano"),
                                ano = reader.GetInt32("ano"),
                                eskaera_kopurua = reader.GetInt32("eskaera_kopurua"),
                                fakturazioa_total = reader.GetDecimal("fakturazioa_total")
                            };
                            result.Add(row);
                        }
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetAlmazena")]
        public async Task<IActionResult> GetAlmazena()
        {
            try
            {
                var result = new List<object>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                    SELECT id, izena, mota, stock
                    FROM almazena;";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())

                    while (await reader.ReadAsync()){
                        var row = new
                        {
                            id = reader["id"],
                            izena = reader["izena"],
                            mota = reader["mota"],
                            stock = reader["stock"]
                        };
                        result.Add(row);
                    }
                }

                return Ok(result); // Devuelve la lista en formato JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // Devuelve un error si ocurre
            }
        }

        [HttpGet("GetPlatera")]
        public async Task<IActionResult> GetPlatera()
        {
            try
            {
                var result = new List<object>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                    SELECT id, izena, deskribapena, mota, platera_mota, prezioa, menu
                    FROM platera;";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())

                    while (await reader.ReadAsync())
                    {
                        var row = new
                        {
                            id = reader["id"],
                            izena = reader["izena"],
                            deskribapena = reader["deskribapena"],
                            mota = reader["mota"],
                            platera_mota = reader["platera_mota"],
                            prezioa = reader["prezioa"],
                            menu = reader["menu"]
                        };
                        result.Add(row);
                    }
                }

                return Ok(result); // Devuelve la lista en formato JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // Devuelve un error si ocurre
            }
        }

        [HttpGet("GetPlatosConIngredientes")]
        public async Task<IActionResult> GetPlatosConIngredientes()
        {
            try
            {
                var platosConIngredientes = new Dictionary<string, List<string>>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 
                            p.izena AS plato,
                            a.izena AS ingrediente
                        FROM almazena_platera ap
                        JOIN platera p ON ap.platera_id = p.id
                        JOIN almazena a ON ap.almazena_id = a.id
                        ORDER BY p.izena, a.izena;
                    ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string plato = reader["plato"].ToString();
                            string ingrediente = reader["ingrediente"].ToString();

                            if (!platosConIngredientes.ContainsKey(plato))
                            {
                                platosConIngredientes[plato] = new List<string>();
                            }
                            platosConIngredientes[plato].Add(ingrediente);
                        }
                    }
                }

                return Ok(platosConIngredientes); // Devuelve la respuesta en formato JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetLangileak")]
        public async Task<IActionResult> GetLangileak()
        {
            try
            {
                var langileak = new List<Dictionary<string, object>>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM langilea;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var langilea = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                langilea[columnName] = value;
                            }

                            langileak.Add(langilea);
                        }
                    }
                }

                return Ok(langileak); // Devuelve la lista de empleados en formato JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetMesas")]
        public async Task<IActionResult> GetMesas()
        {
            try
            {
                var mahaiak = new List<Dictionary<string, object>>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM mahaia;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var mahaia = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                mahaia[columnName] = value;
                            }

                            mahaiak.Add(mahaia);
                        }
                    }
                }

                return Ok(mahaiak); // Devuelve la lista de empleados en formato JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetPedidosPlatos")]
        public async Task<IActionResult> GetPedidosPlatos()
        {
            try
            {
                var pedidos = new Dictionary<int, Dictionary<string, object>>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                SELECT 
                    e.id AS eskaera_id,
                    l.izena AS camarero,
                    p.izena AS plato,
                    p.prezioa AS precio,
                    ep.ateratze_ordua AS hora_entrega
                FROM eskaera_platera ep
                JOIN eskaera e ON ep.eskaera_id = e.id
                JOIN langilea l ON e.langilea_id = l.id
                JOIN platera p ON ep.platera_id = p.id
                ORDER BY e.id;
            ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int pedidoId = reader.GetInt32("eskaera_id");
                            string camarero = reader.GetString("camarero");
                            string plato = reader.GetString("plato");
                            decimal precio = reader.GetDecimal("precio");

                            // Manejar NULL en "ateratze_ordua"
                            string horaEntrega = reader.IsDBNull(reader.GetOrdinal("hora_entrega"))
                                ? ""  // Si es NULL, asignamos una cadena vacía
                                : reader.GetDateTime("hora_entrega").ToString("yyyy-MM-dd HH:mm:ss");

                            // Si el pedido no existe en el diccionario, lo creamos
                            if (!pedidos.ContainsKey(pedidoId))
                            {
                                pedidos[pedidoId] = new Dictionary<string, object>
                        {
                            { "id_pedido", pedidoId },
                            { "camarero", camarero },
                            { "platos", new List<Dictionary<string, object>>() },
                            { "hora_entrega", horaEntrega },
                            { "precio_total", 0m } // Inicializamos el precio total en 0
                        };
                            }

                            // Añadimos el plato con su precio a la lista de platos de ese pedido
                            var platoInfo = new Dictionary<string, object>
                    {
                        { "nombre", plato },
                        { "precio", precio }
                    };

                            ((List<Dictionary<string, object>>)pedidos[pedidoId]["platos"]).Add(platoInfo);

                            // Sumamos el precio al total
                            pedidos[pedidoId]["precio_total"] = (decimal)pedidos[pedidoId]["precio_total"] + precio;
                        }
                    }
                }

                return Ok(pedidos.Values); // Convertimos el diccionario en lista JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("GetPlatosMasPedidos")]
        public async Task<IActionResult> GetPlatosMasPedidos()
        {
            try
            {
                var platos = new List<object>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 
                            p.izena AS plato,
                            COUNT(ep.platera_id) AS cantidad_pedidos
                        FROM eskaera_platera ep
                        JOIN platera p ON ep.platera_id = p.id
                        GROUP BY ep.platera_id, p.izena
                        ORDER BY cantidad_pedidos DESC;
                    ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new
                            {
                                plato = reader.GetString("plato"),
                                cantidad_pedidos = reader.GetInt32("cantidad_pedidos")
                            };
                            platos.Add(row);
                        }
                    }
                }

                return Ok(platos); // Retorna la lista de platos más pedidos en JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetPedidosPorCamareroPorDia")]
        public async Task<IActionResult> GetPedidosPorCamareroPorDia()
        {
            try
            {
                var pedidosPorCamarero = new List<object>();

                using (MySqlConnection connection = GetDbConnection())
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 
                            l.izena AS camarero,
                            DATE(ep.eskaera_ordua) AS fecha,
                            COUNT(DISTINCT CONCAT(ep.eskaera_id, '-', ep.eskaera_ordua)) AS cantidad_pedidos
                        FROM eskaera e
                        JOIN langilea l ON e.langilea_id = l.id
                        JOIN eskaera_platera ep ON e.id = ep.eskaera_id
                        GROUP BY l.izena, fecha
                        ORDER BY fecha DESC, cantidad_pedidos DESC;
                    ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new
                            {
                                camarero = reader.GetString("camarero"),
                                fecha = reader.GetDateTime("fecha").ToString("yyyy-MM-dd"),
                                cantidad_pedidos = reader.GetInt32("cantidad_pedidos")
                            };
                            pedidosPorCamarero.Add(row);
                        }
                    }
                }

                return Ok(pedidosPorCamarero); // Retorna la lista en JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }



    }
}

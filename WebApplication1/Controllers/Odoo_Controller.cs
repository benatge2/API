﻿using Microsoft.AspNetCore.Mvc;
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

        // Método para obtener la conexión a la base de datos
        private MySqlConnection GetDbConnection()
        {
            string connectionString = $"Server={databaseIp};Port={port};Database={databaseName};User={databaseUser};Password={databasePassword};";
            return new MySqlConnection(connectionString);
        }

        [HttpGet("GetEskaeraKopurua")]
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
                            COUNT(DISTINCT eskaera_id) AS eskaera_kopurua,
                            DAYNAME(eskaera_ordua) AS eguna
                        FROM 
                            eskaera_platera
                        GROUP BY 
                            DAYNAME(eskaera_ordua)
                        ORDER BY 
                            MIN(FIELD(DAYNAME(eskaera_ordua), 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'));";


                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (var reader = (MySqlDataReader)await command.ExecuteReaderAsync())

                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new
                            {
                                eskaera_kopurua = reader["eskaera_kopurua"],
                                eguna = reader["eguna"].ToString()
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

    }
}

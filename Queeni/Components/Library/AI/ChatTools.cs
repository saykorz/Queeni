using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Queeni.Components.Library.AI
{
    public static class ChatTools
    {
        public static object GetCreateCategoryFunction()
        {
            return new
            {
                type = "function",
                function = new
                {
                    name = "CreateCategory",
                    description = "Creates a new category and returns whether it was successful.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            title = new
                            {
                                type = "string",
                                description = "The name of the category to create, e.g., 'Домашни' or 'Работа'."
                            }
                        },
                        required = new[] { "title" },
                        additionalProperties = false
                    },
                    strict = true
                }
            };
        }

        public static object GetCreateCategoryResponsesFunction()
        {
            return new
            {
                type = "function",
                name = "CreateCategory",
                description = "Creates a new category and returns whether it was successful.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        title = new
                        {
                            type = "string",
                            description = "The name of the category to create, e.g., 'Домашни' or 'Работа'."
                        }
                    },
                    required = new[] { "title" },
                    additionalProperties = false
                },
                strict = true
            };
        }

        public static object GetCreateTaskFunction()
        {
            return new
            {
                type = "function",
                function = new
                {
                    name = "CreateTask",
                    description = "Creates a new task with title, description, optional color, tags, and category.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            title = new
                            {
                                type = "string",
                                description = "The title of the task, e.g., 'Write report'."
                            },
                            description = new
                            {
                                type = "string",
                                description = "Detailed description of the task."
                            },
                            priority = new
                            {
                                type = "string",
                                description = "Priority level of the task. Use 'Red' for high, 'Blue' for normal, and 'Black' for low priority.",
                            },
                            tags = new
                            {
                                type = "string",
                                description = "Comma-separated tags for the task. Optional.",
                            },
                            categoryId = new
                            {
                                type = "integer",
                                description = "The id of the category if exist, e.g., 1, 2"
                            },
                            categoryTitle = new
                            {
                                type = "string",
                                description = "The name of the category this task belongs to, e.g., 'Work'. Optional."
                            }
                        },
                        required = new[] { "title", "description", "priority", "tags", "categoryId", "categoryTitle" },
                        additionalProperties = false
                    },
                    strict = true
                }
            };
        }

        public static object GetCreateTaskResponsesFunction()
        {
            return new
            {
                type = "function",
                name = "CreateTask",
                description = "Creates a new task with title, description, optional color, tags, and category.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        title = new
                        {
                            type = "string",
                            description = "The title of the task, e.g., 'Write report'."
                        },
                        description = new
                        {
                            type = "string",
                            description = "Detailed description of the task."
                        },
                        priority = new
                        {
                            type = "string",
                            description = "Priority level of the task. Use 'Red' for high, 'Blue' for normal, and 'Black' for low priority.",
                        },
                        tags = new
                        {
                            type = "string",
                            description = "Comma-separated tags for the task. Optional.",
                        },
                        categoryId = new
                        {
                            type = "integer",
                            description = "The id of the category if exist, e.g., 1, 2"
                        },
                        categoryTitle = new
                        {
                            type = "string",
                            description = "The name of the category this task belongs to, e.g., 'Work'. Optional."
                        }
                    },
                    required = new[] { "title", "description", "priority", "tags", "categoryId", "categoryTitle" },
                    additionalProperties = false
                },
                strict = true
            };
        }

        public static object GetDeviceLocationAsync()
        {
            //try
            //{
            //    var location = await Geolocation.GetLastKnownLocationAsync();
            //    if (location != null)
            //    {
            //        await GetLocationFromCoordinatesAsync(location.Latitude, location.Longitude);

            //        var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
            //        var placemark = placemarks?.FirstOrDefault();

            //        if (placemark != null)
            //        {
            //            var tool = new
            //            {
            //                type = "web_search_preview",
            //                search_context_size = "low",
            //                user_location = new
            //                {
            //                    type = "approximate",
            //                    country = placemark.CountryName,
            //                    city = placemark.Locality,
            //                    region = placemark.Locality
            //                }
            //            };

            //            return tool;
            //        }
            //    }
            //}
            //catch (FeatureNotSupportedException fnsEx)
            //{
            //    // Handle not supported on device
            //}
            //catch (PermissionException pEx)
            //{
            //    // Handle permission exception
            //}
            //catch (Exception ex)
            //{
            //    // Handle other errors
            //}
         
            return new
            {
                type = "web_search_preview",
                //search_context_size = "low",
            };
        }
    }
}

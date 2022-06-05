﻿//   Copyright © 2022 Vaughn Vernon. All rights reserved.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VaughnVernon.Mockroservices.Serializations
{
    public class Serialization
    {
        public static string Serialize(object? instance)
        {
            var serialization = JsonConvert.SerializeObject(instance, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            return serialization;
        }

        public static T Deserialize<T>(string serialization)
        {
            var deserialized = JsonConvert.DeserializeObject<T>(serialization, new JsonSerializerSettings
            {
                //ContractResolver = new CamelCasePropertyNamesContractResolver()
                ContractResolver = new PrivateSetterCamelCasePropertyNamesContractResolver()
            });
            return deserialized!;
        }

        public static object Deserialize(string serialization, Type type)
        {
            var deserialized = JsonConvert.DeserializeObject(serialization, type, new JsonSerializerSettings
            {
                //ContractResolver = new CamelCasePropertyNamesContractResolver()
                ContractResolver = new PrivateSetterCamelCasePropertyNamesContractResolver()
            });

            return deserialized!;
        }
    }
}

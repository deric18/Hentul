namespace FirstOrderMemory.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class NeuronConverter : JsonConverter<Neuron>
    {
        public override Neuron? ReadJson(JsonReader reader, Type objectType, Neuron? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            uint pruneCount = (uint)jo["prunecount"];
            string NeuronID = (string)jo["neuronID"];
            string nType = (string)jo["ntype"];
            uint lastSpikeCycleNum = (uint)jo["lastspikecyclenum"];

            Dictionary<string, Synapse> axonaList = FixSynapseLists(jo["axonalist"].ToObject<Dictionary<string, Synapse>>(serializer), true);
            Dictionary<string, Synapse> ProximoDistalDendriticList = FixSynapseLists(jo["proximodistaldendriticlist"].ToObject<Dictionary<string, Synapse>>(serializer), false, NeuronID);

            string neuronstate = (string)jo["currentstate"];
            int flag = (int)jo["flag"];
            int voltage = (int)jo["voltage"];

            return new Neuron(pruneCount, NeuronID, nType, ProximoDistalDendriticList, axonaList, neuronstate, voltage, flag, lastSpikeCycleNum);
        }

        public override void WriteJson(JsonWriter writer, Neuron? value, JsonSerializer serializer)
        {
            JObject jo = new JObject
            {
                { "prunecount", value.PruneCount },
                { "neuronID", value.NeuronID.ToString() },
                { "ntype", value.nType.ToString() },
                { "lastspikecyclenum", value.lastSpikeCycleNum.ToString() },
                { "axonalist", JToken.FromObject(value.AxonalList)  },
                { "proximodistaldendriticlist", JToken.FromObject(value.ProximoDistalDendriticList) },
                { "currentstate", value.CurrentState.ToString() },
                { "flag", value.flag.ToString() },
                { "voltage", value.Voltage.ToString() }
            };

            jo.WriteTo(writer);
        }

        private Dictionary<string, Synapse> FixSynapseLists(Dictionary<string, Synapse> adList, bool isAxonalList, string neuronID = "")
        {
            Dictionary<string, Synapse> listToReturn;

            listToReturn = adList;

            foreach (var kvp in listToReturn)
            {
                if (kvp.Value?.DendronalNeuronalId == null)
                {
                    kvp.Value.SetDendronalID(isAxonalList ?  kvp.Key : neuronID);
                }
            }
           
            return listToReturn;
        }
    }
}

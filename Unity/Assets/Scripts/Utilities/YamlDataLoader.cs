﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;
using YamlDotNet.Serialization;
using MiniJSON;
using Models;


public static class YamlDataLoader
{
    /// <summary>
    /// YAMLファイルからデータをロードして、MessageModelリストを生成する
    /// </summary>
    /// <param name="path">読み込み元YAMLファイルパス</param>
    /// <returns>List<SimulationModel></returns>
    public static SimulationModel LoadAllMessageModelsFromYamlFile(string path)
    {
        var simulationModel = new SimulationModel();
        var allMessages = new Dictionary<string, MessageModel>();
        
        var slots = new Dictionary<int, Dictionary<string, ValidatorModel>>();
        var sr = new StreamReader(path);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize(sr);
        var serializer = new SerializerBuilder()
            .JsonCompatible()
            .Build();
        
        var jsonStr = serializer.Serialize(yamlObject);

        var slotList = (IList)Json.Deserialize(jsonStr);
        foreach (IDictionary slot in slotList)
        {
            var validatorMap = new Dictionary<string, ValidatorModel>();
            var slotNo = int.Parse((string)slot["slot"]);
            var validators = (IList) slot["validators"];
            foreach (IDictionary validator in validators)
            {
                var validatorName = (string) validator["name"];
                var state = (IDictionary) validator["state"];
                
                var messageModels = new List<MessageModel>();
                var messages = (IList) state["messages"];
                foreach (IDictionary message in messages)
                {
                    var hash = (string)message["hash"];
                    var parentHash = (string)message["parent_hash"];
                    var receiverSlot = int.Parse((string)message["receiver_slot"]);
                    var sender = (string)message["sender"];
                    var senderSlot = int.Parse((string) message["sender_slot"]);
                    var estimate = (IDictionary) message["estimate"];
                    
                    
                    var estimatedHash = (string) estimate["hash"];
                    var estimatedParentHash = (string) estimate["parent_hash"];
                    var estimateModel = new EstimateModel(estimatedHash, estimatedParentHash);

                    var justificationMessages = new List<MessageModel>();
                    var justification = (IList) message["justification"];
                    foreach (IDictionary m in justification)
                    {
                        var justificationHash = (string)m["message_hash"];
                        var justificationSender = (string) m["sender"];
                        justificationMessages.Append(new MessageModel(justificationHash, justificationSender));
                    }
                    var messageModel = new MessageModel(hash, sender, parentHash, receiverSlot, senderSlot,
                        estimateModel, justificationMessages);
                    messageModels.Append(messageModel);
                    allMessages[messageModel.Hash] = messageModel;
                }
                var stateModel = new StateModel(messageModels);
                var validatorModel = new ValidatorModel(validatorName, stateModel);
                validatorMap[validatorName] = validatorModel;
            }
            slots[slotNo] = validatorMap;
        }
        Debug.Log("finish");
        
        simulationModel.Slots = slots;
        simulationModel.AllMessages = allMessages;
        return simulationModel;
    }
}


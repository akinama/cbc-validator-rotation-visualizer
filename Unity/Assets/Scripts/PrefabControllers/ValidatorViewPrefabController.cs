﻿using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx.Async;
using UnityEngine;

public class ValidatorViewPrefabController : MonoBehaviour
{
    /// <summary>
    /// Prefab Path
    /// </summary>
    private const string PrefabPath = "Prefabs/ValidatorView";

    /// <summary>
    /// Prefab Path
    /// </summary>
    private const string VerticalLayoutPrefabPath = "Prefabs/ValidatorVerticalLayoutGroup";

    /// <summary>
    /// MessageModels (Dictionary key is slot no)
    /// </summary>
    public Dictionary<int, List<MessageModel>> MessageModels { get; private set; }

    /// <summary>
    /// EdgeModels (Dictionary key is slot no)
    /// </summary>
    public Dictionary<int, List<EdgeModel>> EdgeBySlot { get; private set; }

    /// <summary>
    /// Current Slot
    /// </summary>
    private int currentSlot;

    /// <summary>
    /// HorizontalLayoutGroupTransform
    /// </summary>
    [SerializeField] private Transform horizontalLayoutGroupTransform;

    /// <summary>
    /// VerticalLayoutGroups
    /// </summary>
    [SerializeField] private List<GameObject> verticalLayoutGroups;

    /// <summary>
    /// Message Prefab Controllers
    /// </summary>
    [SerializeField] private List<MessagePrefabController> messagePrefabControllers;
    
    /// <summary>
    /// Validator Name
    /// </summary>
    [SerializeField] private TextMeshProUGUI validatorName;
    
    /// <summary>
    /// Validator ViewのInstantiate
    /// </summary>
    public static ValidatorViewPrefabController InstantiatePrefab(string validatorName, int slot, Dictionary<int, List<MessageModel>> messageModels, Dictionary<int, List<EdgeModel>> edgeBySlot)
    {
        var gameObject = Instantiate(Resources.Load<GameObject>(PrefabPath));

        var validatorViewPrefabController = gameObject.GetComponent<ValidatorViewPrefabController>();

        validatorViewPrefabController.Initialize(validatorName, slot, messageModels, edgeBySlot);

        return validatorViewPrefabController;
    }

    /// <summary>
    /// 初期化処理を行う
    /// </summary>
    private void Initialize(string validatorName, int slot, Dictionary<int, List<MessageModel>> messageModels, Dictionary<int, List<EdgeModel>> edgeBySlot)
    {
        this.validatorName.SetText(validatorName);

        currentSlot = slot;
        
        MessageModels = messageModels;
        
        EdgeBySlot = edgeBySlot;

        UpdateBySlot(slot);
    }

    /// <summary>
    /// 特定スロットの状態にViewをアップデートする
    /// </summary>
    public void UpdateBySlot(int slot)
    {
        foreach (var verticalLayoutGroup in verticalLayoutGroups)
        {
            Destroy(verticalLayoutGroup);
        }
        
        verticalLayoutGroups.Clear();
       
        foreach (var messagePrefabController in messagePrefabControllers)
        {
            Destroy(messagePrefabController.gameObject);
        }
        
        messagePrefabControllers.Clear();
        
        foreach (var messageModel in MessageModels[slot])
        {
            var verticalLayoutGroupGameObject = Instantiate(Resources.Load<GameObject>(VerticalLayoutPrefabPath));
            
            verticalLayoutGroupGameObject.transform.SetParent(horizontalLayoutGroupTransform, false);
            
            verticalLayoutGroups.Add(verticalLayoutGroupGameObject);

            var messagePrefabController = MessagePrefabController.InstantiatePrefab(messageModel);
            
            messagePrefabControllers.Add(messagePrefabController);
            
            messagePrefabController.transform.SetParent(verticalLayoutGroupGameObject.transform, false);
        }

        foreach (var edgeModel in EdgeBySlot[slot])
        {
            foreach (var messagePrefabController in messagePrefabControllers.Where(prefabController => prefabController.MessageModel == edgeModel.SrcMsg))
            {
                var destinationMessagePrefabController = messagePrefabControllers.FirstOrDefault(prefabController => prefabController.MessageModel == edgeModel.DstMsg);

                if (destinationMessagePrefabController == null)
                {
                    continue;
                }

                EdgePrefabController.InstantiatePrefabAsync(messagePrefabController, destinationMessagePrefabController).Forget();
            }
        }
    }
}
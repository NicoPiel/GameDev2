﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Simulation.Modules.CustomerSimulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Simulation.Core
{
    public class SimulationManager : MonoBehaviour
    {
        // Public
        public TMP_Text timeDisplay;
        public int timeValue;
        public int startOfDay;
        public int endOfDay;
        public int durationOfDayInMinutes;

        public static Dictionary<string, string[]> Names { get; set; }
        public static Dictionary<string, string[]> Orders { get; set; }
        public static List<string> Attributes { get; set; }
        public static Dictionary<string, string[]> AttributeCombinations { get; set; }

        // Private
        private static SimulationManager _instance;
        [SerializeField] private CustomerSimulation customerSimulation;
        private static readonly string PathToNameJson = Application.streamingAssetsPath + "/JSON/names.json";
        private static readonly string PathToOrdersJson = Application.streamingAssetsPath + "/JSON/orders.json";
        private static readonly string PathToAttributesJson = Application.streamingAssetsPath + "/JSON/attributes.json";

        // Events
        public static UnityEvent onSimulationStart;
        public static UnityEvent onSimulationPause;
        public static UnityEvent onSimulationUnpause;
        public static UnityEvent onSimulationTick;

        private bool _paused;
        private float _tickDuration = 1f;
        private int _durationOfDay;

        // Start is called before the first frame update
        private void Awake()
        {
            // Reference static instance
            _instance = this;

            // Setup Events
            onSimulationStart = new UnityEvent();
            onSimulationPause = new UnityEvent();
            onSimulationUnpause = new UnityEvent();
            onSimulationTick  = new UnityEvent();
            
            // JSON files
            Names = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(PathToNameJson));
            Orders = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(PathToOrdersJson));
            AttributeCombinations = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(PathToAttributesJson));
            Attributes = new List<string>();

            foreach (var key in AttributeCombinations.Keys)
            {
                Attributes.Add(key);
            }
        }
    
        private void Start()
        {
            // Subscribe to own events for debugging
            onSimulationStart.AddListener(OnSimulationStart);
            onSimulationPause.AddListener(OnSimulationPause);
            onSimulationUnpause.AddListener(OnSimulationUnpause);
            onSimulationTick.AddListener(OnSimulationTick);

            _durationOfDay = endOfDay - startOfDay;
        }

        public void StartSimulation()
        {
            onSimulationStart.Invoke();
            StartCoroutine(SimulationTick());
        }

        public void PauseSimulation()
        {
            onSimulationPause.Invoke();
            StopCoroutine(SimulationTick());
        }
        
        public void UnpauseSimulation()
        {
            onSimulationUnpause.Invoke();
            StartCoroutine(SimulationTick());
        }

        // TODO
        private IEnumerator SimulationPreTick()
        {
            yield return null;
        }

        private IEnumerator SimulationTick()
        {
            for (;;)
            {
                if (_paused) break;

                onSimulationTick.Invoke();
                yield return new WaitForSeconds(_tickDuration);
            }
        }

        // TODO
        private IEnumerator SimulationPostTick()
        {
            yield return null;
        }

        private void OnSimulationStart()
        {
            _paused = false;
            timeValue = startOfDay;
            Debug.Log("Simulation started.");
        }

        private void OnSimulationPause()
        {
            _paused = true;
            Debug.Log("Simulation paused.");
        }
        
        private void OnSimulationUnpause()
        {
            _paused = false;
            Debug.Log("Simulation unpaused.");
        }

        private void OnSimulationTick()
        {
            Timelapse();
            Debug.Log("Tick.");
        }

        private void Timelapse()
        {
            timeValue += Mathf.RoundToInt((float) _durationOfDay / durationOfDayInMinutes / _tickDuration / 60);

            var hours = timeValue / 60;
            var minutes = timeValue % 60;

            var hoursString = hours < 10 ? $"0{hours}" : $"{hours}";
            var minutesString = minutes < 10 ? $"0{minutes}" : $"{minutes}";
            
            timeDisplay.text = $"{hoursString}:{minutesString}";
        }
        
        public static SimulationManager GetInstance()
        {
            return _instance;
        }

        public static CustomerSimulation CustomerSimulation()
        {
            return _instance.customerSimulation;
        }
    }
}

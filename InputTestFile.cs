using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeyInputTestProgram {

    public class KeyAction {
        public string Type { get; set; }    // "any-key" or "and-key"
        public string Action { get; set; }  // Action to perform, e.g., "swap-kbm", "swap-pad"
        public List<List<string>> Keys { get; set; }  // Key conditions

        public KeyAction(string type, string action, List<List<string>> keys) {
            Type = type;
            Action = action;
            Keys = keys;
        }
    }


    internal static class _EntryPoint {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void _EntryPointVoid() {

            List<KeyAction> keyActions = new List<KeyAction> {
                // Swap to kbm on CTRL+ALT+5
                new KeyAction("any-key", "swap-kbm (CTRL+ALT+5)", new List<List<string>> {
                    new List<string> { "kbm:CTRL", "kbm:ALT", "kbm:5" }
                }),

                // Swap to kbm on LS (click) + Y
                new KeyAction("any-key", "swap-pad (Y || RS)", new List<List<string>> {
                    new List<string> { "pad:Y" },
                    new List<string> { "pad:RS" }
                }),

                // Swap to keyboard UI
                new KeyAction("any-key", "swap-kbm (WASD)", new List<List<string>> {
                    new List<string> { "kbm:W" },
                    new List<string> { "!kbm:ALT", "kbm:S" },
                    new List<string> { "!kbm:ALT", "kbm:A" },
                    new List<string> { "kbm:D" },

                }),

                // Toggle controller/kb on F9 or LS (click) + Y
                new KeyAction("any-key", "toggle (F9 || LS+Y)", new List<List<string>> {
                    new List<string> { "kbm:F9" },
                    new List<string> { "pad:LS", "pad:Y" }
                }),

                // Swap to Pad UI (on stick, use configured deadzone) 
                new KeyAction("any-key", "swap-kbm (LSV || RSV)", new List<List<string>> {
                    new List<string> { "pad:LSV" },
                    new List<string> { "pad:RSV" }
                }),

                // Swap to KB ui when joystick is not moving
                new KeyAction("any-key", "swap-kbm (**V<=10)", new List<List<string>> {
                    new List<string> { "pad:LSV<=10" },
                    new List<string> { "pad:RSV<=10" }
                }),
                
                // Swap to KB ui when joystick is not moving
                new KeyAction("any-key", "swap-pad (**V>10)", new List<List<string>> {
                    new List<string> { "pad:LSV>10" },
                    new List<string> { "pad:RSV>10" }
                })
            };

            HashSet<(string testMessage, HashSet<string> pressedKeys, Dictionary<string, float> axisValues)> tests = new HashSet<(string, HashSet<string>, Dictionary<string, float>)>
            {
                // ("CTRL, ALT, 5, pad:X, pad:LS", new HashSet<string> {
                //     "kbm:CTRL", "kbm:ALT", "kbm:5", "pad:X", "pad:LS"
                // }, new Dictionary<string, float> { { "LSV", 15.0f }, { "RSV", 0.0f }, { "LT", 0.0f }, { "RT", 0.0f } }),
                // 
                // ("Keyboard player movement, strafe W and A", new HashSet<string> {
                //     "kbm:W", "kbm:A"
                // }, new Dictionary<string, float> { { "LSV", 0.0f }, { "RSV", 0.0f }, { "LT", 0.0f }, { "RT", 0.0f } }),
                // 
                ("Keyboard player movement, pressing A (without alt)", new HashSet<string> {
                    "kbm:A"
                }, new Dictionary<string, float> { { "LSV", 0.0f }, { "RSV", 0.0f }, { "LT", 0.0f }, { "RT", 0.0f } }),

                ("Keyboard player movement, pressing A (WITH alt)", new HashSet<string> {
                    "kbm:A", "kbm:ALT"
                }, new Dictionary<string, float> { { "LSV", 0.0f }, { "RSV", 0.0f }, { "LT", 0.0f }, { "RT", 0.0f } }),
            };

            GameLoop gameLoop = new GameLoop(keyActions);

            //while(true)
               
            foreach(var test in tests) {
                Console.WriteLine(test.testMessage + ":");
                gameLoop.Update(test.axisValues, test.pressedKeys);
                Console.WriteLine();
            }


            Console.ReadKey();
        }
    }

    public class GameLoop {
        public float CachedConfigValue_GlobalDeadzone = 20;
        private List<KeyAction> keyActions = new List<KeyAction>();

        public GameLoop(List<KeyAction> keyActions) {
            this.keyActions = keyActions;
        }

        public void Update(Dictionary<string, float> axisValues, HashSet<string> pressedKeys) {
            // TODO: Update CachedConfigValue_GlobalDeadzone

            int count = keyActions.Count;
            for (int i = 0; i < count; i++) {
                KeyAction keyAction = keyActions[i];
                bool actionTriggered = false;

                if (keyAction.Type == "any-key") {
                    actionTriggered = CheckAndKeyAny(keyAction, axisValues, pressedKeys);
                }
                else if (keyAction.Type == "and-key") {
                    actionTriggered = CheckAndKeyAnd(keyAction, axisValues, pressedKeys);
                }

                if (actionTriggered) {
                    ExecuteAction(keyAction.Action);
                }
            }
        }

        public static bool AllBoolsTrue(bool[] array) {
            for (int i = 0; i < array.Length; i++)
                if (!array[i])
                    return false;
            return true;
        }

        private bool CheckAndKeyAny(KeyAction keyAction, Dictionary<string, float> axisValues, HashSet<string> pressedKeys) {
            foreach (var keySet in keyAction.Keys) {
                bool[] keyConditions = new bool[keySet.Count];
                var allKeysDebug = string.Join(", ", keySet);

                for (var idx = 0; idx < keySet.Count; idx++) {
                    var key = keySet[idx];
                    keyConditions[idx] = EvaluateCondition(key, axisValues, pressedKeys);
                }

                if (AllBoolsTrue(keyConditions))
                    return true;
            }
            return false;
        }

         private bool CheckAndKeyAnd(KeyAction keyAction, Dictionary<string, float> axisValues, HashSet<string> pressedKeys) {
            bool[] keySets = new bool[keyAction.Keys.Count];
            for (var keySetIdx = 0; keySetIdx < keyAction.Keys.Count; keySetIdx++) {
                var keySet = keyAction.Keys[keySetIdx];
                bool[] keyConditions = new bool[keySet.Count];

                for (var keyIdx = 0; keyIdx < keySet.Count; keyIdx++) {
                    var key = keySet[keyIdx];
                    keyConditions[keyIdx] = EvaluateCondition(key, axisValues, pressedKeys);
                }

                keySets[keySetIdx] = (AllBoolsTrue(keyConditions));
            }

            return (AllBoolsTrue(keySets)); ;
        }

        private bool EvaluateCondition(string condition, Dictionary<string, float> axisValues, HashSet<string> pressedKeys) {
            var inverseCondition = condition[0] == '!';
            if (inverseCondition) condition = condition.Substring(1);

            if (condition.StartsWith("pad:LSV") || condition.StartsWith("pad:RSV") 
                    || condition.StartsWith("pad:LT") || condition.StartsWith("pad:RT")) {
                // Handle axis conditions, e.g., "pad:LSV>=20"
                var result = EvaluateAxisCondition(condition, axisValues);
                return (inverseCondition) ? !result : result;
            }
            else {
                // Handle keyboard and mouse conditions
                var result = pressedKeys.Contains(condition);
                return (inverseCondition) ? !result : result;
            }
        }

        private bool EvaluateAxisCondition(string condition, Dictionary<string, float> axisValues) {
            // Example: "pad:LSV>=20"
            string[] parts = condition.Split(new[] { "pad:", ">=", "<=", ">", "<" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 2) return false;

            string axis = parts[0]; // Extract the axis name
            float value = parts.Length == 2 ? float.Parse(parts[1]) : CachedConfigValue_GlobalDeadzone; // Extract the threshold value

            if (!axisValues.TryGetValue(axis, out float axisValue)) {
                return false;
            }
            else if (condition.Contains(">=")) {
                return axisValue >= value;
            }
            else if (condition.Contains("<=")) {
                return axisValue <= value;
            }
            else if (condition.Contains(">")) {
                return axisValue > value;
            }
            else if (condition.Contains("<")) {
                return axisValue < value;
            }

            return false;
        }

        private void ExecuteAction(string action) {
            // Implement the logic to execute the action
            Console.WriteLine($"Action triggered: {action}");
        }
    }

}

using AmeisenAI;
using AmeisenAI.Combat;
using AmeisenManager;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static AmeisenAI.Combat.CombatStructures;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für CombatClassEditor.xaml
    /// </summary>
    public partial class CombatClassEditor : Window
    {
        private AmeisenBotManager BotManager { get; }
        private CombatLogic loadedLogic;
        private int prio;

        public CombatClassEditor()
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.Instance;

            string defaultCombatClass = BotManager.Settings.combatClassPath;
            if (defaultCombatClass != "none")
                loadedLogic = CombatEngine.LoadCombatLogicFromFile(defaultCombatClass);
            else
                loadedLogic = new CombatLogic();
        }

        private void GuiCombatClassEditor_Loaded(object sender, RoutedEventArgs e)
        {
            comboboxAction.Items.Add(CombatLogicAction.USE_SPELL);
            comboboxAction.Items.Add(CombatLogicAction.USE_AOE_SPELL);
            comboboxAction.Items.Add(CombatLogicAction.SHAPESHIFT);
            comboboxAction.Items.Add(CombatLogicAction.FLEE);

            comboboxValueOne.Items.Add(CombatLogicValues.MYSELF_HP);
            comboboxValueOne.Items.Add(CombatLogicValues.MYSELF_ENERGY);
            comboboxValueOne.Items.Add(CombatLogicValues.TARGET_HP);
            //comboboxValueOne.Items.Add(CombatLogicValues.TARGET_IS_CASTING);

            comboboxValueTwo.Items.Add(CombatLogicValues.MYSELF_HP);
            comboboxValueTwo.Items.Add(CombatLogicValues.MYSELF_ENERGY);
            comboboxValueTwo.Items.Add(CombatLogicValues.TARGET_HP);
            //comboboxValueTwo.Items.Add(CombatLogicValues.TARGET_IS_CASTING);

            comboboxValueOperator.Items.Add(CombatLogicStatement.GREATER);
            comboboxValueOperator.Items.Add(CombatLogicStatement.GREATER_OR_EQUAL);
            comboboxValueOperator.Items.Add(CombatLogicStatement.EQUAL);
            comboboxValueOperator.Items.Add(CombatLogicStatement.LESS_OR_EQUAL);
            comboboxValueOperator.Items.Add(CombatLogicStatement.LESS);
            comboboxValueOperator.Items.Add(CombatLogicStatement.HAS_BUFF);
            comboboxValueOperator.Items.Add(CombatLogicStatement.HAS_BUFF_MYSELF);
            comboboxValueOperator.Items.Add(CombatLogicStatement.NOT_HAS_BUFF);
            comboboxValueOperator.Items.Add(CombatLogicStatement.NOT_HAS_BUFF_MYSELF);

            comboboxActionType.Items.Add(CombatActionType.ATTACK);
            comboboxActionType.Items.Add(CombatActionType.HEAL);
            comboboxActionType.Items.Add(CombatActionType.TANK);

            LoadEntries();
        }

        private void GuiCombatClassEditor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                loadedLogic.combatLogicEntries.Clear();
                loadedLogic = CombatEngine.LoadCombatLogicFromFile(openFileDialog.FileName);

                if (loadedLogic.combatLogicEntries != null)
                {
                    listboxCombatActions.Items.Clear();
                    listboxConditions.Items.Clear();
                    foreach (CombatLogicEntry action in loadedLogic.combatLogicEntries)
                        listboxCombatActions.Items.Add(action);
                    listboxCombatActions.SelectedIndex = 0;
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            loadedLogic.combatLogicEntries.Clear();
            foreach (CombatLogicEntry action in listboxCombatActions.Items)
                loadedLogic.combatLogicEntries.Add(action);

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            string defaultCombatClass = BotManager.Settings.combatClassPath;
            if (defaultCombatClass == "none")
                saveFileDialog.FileName = "sampleCombatClass.json";
            else
                saveFileDialog.FileName = defaultCombatClass + ".json";

            if (saveFileDialog.ShowDialog() == true)
                CombatEngine.SaveToFile(saveFileDialog.FileName, loadedLogic);
        }

        private void ListboxCombatActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CombatLogicEntry entry = ((CombatLogicEntry)listboxCombatActions.SelectedItem);
            textboxPriority.Text = entry.Priority.ToString();
            textboxMaxDistance.Text = entry.MaxSpellDistance.ToString();
            textboxSpellName.Text = (string)entry.Parameters;

            checkboxCanCastDuringMovement.IsChecked = entry.CanMoveDuringCast;
            checkboxCombatOnly.IsChecked = entry.CombatOnly;
            checkboxIsBuff.IsChecked = entry.IsBuff;
            checkboxIsForParty.IsChecked = entry.IsBuffForParty;
            checkboxIsForMySelf.IsChecked = entry.IsForMyself;

            listboxConditions.Items.Clear();
            foreach (AmeisenAI.Combat.Condition c in entry.Conditions)
                listboxConditions.Items.Add(c);

            comboboxActionType.SelectedItem = entry.ActionType;
        }

        private void ButtonAddCombatEntry_Click(object sender, RoutedEventArgs e)
        {
            listboxCombatActions.Items.Add(new CombatLogicEntry());
            listboxCombatActions.SelectedIndex = 0;

            textboxPriority.Text = prio.ToString();
            prio++;
        }

        private void ButtonRemoveCombatEntry_Click(object sender, RoutedEventArgs e)
        {
            listboxCombatActions.Items.RemoveAt(listboxCombatActions.SelectedIndex);
        }

        private void TextboxSpellName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).Parameters = textboxSpellName.Text;
        }

        private void TextboxMaxDistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).MaxSpellDistance = float.Parse(textboxMaxDistance.Text);
        }

        private void ComboboxAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).Action = (CombatLogicAction)comboboxAction.SelectedItem;
        }

        private void TextboxPriority_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).Priority = int.Parse(textboxPriority.Text);
        }

        private void CheckboxCombatOnly_Checked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).CombatOnly = true;
        }

        private void CheckboxCombatOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).CombatOnly = false;
        }

        private void CheckboxIsBuff_Checked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).IsBuff = true;
            checkboxIsForParty.IsEnabled = true;
        }

        private void CheckboxIsBuff_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).IsBuff = false;
            checkboxIsForParty.IsEnabled = false;
        }

        private void CheckboxIsForParty_Checked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).IsBuffForParty = true;
        }

        private void CheckboxIsForParty_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).IsBuffForParty = false;
        }

        private void CheckboxCanCastDuringMovement_Checked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).CanMoveDuringCast = true;
        }

        private void CheckboxCanCastDuringMovement_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).CanMoveDuringCast = false;
        }

        private void CheckboxIsForMySelf_Checked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).IsForMyself = true;
        }

        private void CheckboxIsForMySelf_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).IsForMyself = false;
        }

        private void RadiobuttonPreValue_Checked(object sender, RoutedEventArgs e)
        {
            if (textboxCustomValue != null && (AmeisenAI.Combat.Condition)listboxConditions.SelectedItem != null)
            {
                comboboxValueTwo.IsEnabled = true;
                textboxCustomValue.IsEnabled = false;
                ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).customSecondValue = false;
            }
        }

        private void RadiobuttonCustomValue_Checked(object sender, RoutedEventArgs e)
        {
            if (textboxCustomValue != null && (AmeisenAI.Combat.Condition)listboxConditions.SelectedItem != null)
            {
                comboboxValueTwo.IsEnabled = false;
                textboxCustomValue.IsEnabled = true;
                ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).customSecondValue = true;
            }
        }

        private void ListboxConditions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
            {
                if ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem != null)
                {
                    radiobuttonCustomValue.IsChecked = ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).customSecondValue;

                    if (radiobuttonCustomValue.IsChecked == true)
                        textboxCustomValue.Text = ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).conditionValues[1].ToString();
                    else
                        comboboxValueOne.SelectedItem = ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).conditionValues[0];

                    comboboxValueTwo.SelectedItem = ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).conditionValues[1];
                    comboboxValueOperator.SelectedItem = ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).statement;
                }
            }
        }

        private void ButtonAddCondition_Click(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
            {
                AmeisenAI.Combat.Condition condition = new AmeisenAI.Combat.Condition();

                ((CombatLogicEntry)listboxCombatActions.SelectedItem).Conditions.Add(condition);
                listboxConditions.Items.Add(condition);

                listboxConditions.SelectedIndex = 0;
            }
        }

        private void ButtonRemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
            {
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).Conditions.RemoveAt(listboxCombatActions.SelectedIndex);
                listboxConditions.Items.RemoveAt(listboxCombatActions.SelectedIndex);
            }
        }

        private void ComboboxValueOne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listboxConditions.SelectedItem != null)
                ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).conditionValues[0] = (CombatLogicValues)comboboxValueOne.SelectedItem;
        }

        private void ComboboxValueOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listboxConditions.SelectedItem != null)
            {
                CombatLogicStatement op = (CombatLogicStatement)comboboxValueOperator.SelectedItem;
                AmeisenAI.Combat.Condition cond = ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem);

                cond.statement = op;

                listboxConditions.SelectedItem = cond;
            }
        }

        private void ComboboxValueTwo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listboxConditions.SelectedItem != null)
                ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).conditionValues[1] = (CombatLogicValues)comboboxValueTwo.SelectedItem;
        }

        private void TextboxCustomValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (listboxConditions.SelectedItem != null && textboxCustomValue.Text.Length > 0)
                    ((AmeisenAI.Combat.Condition)listboxConditions.SelectedItem).customValue = double.Parse(textboxCustomValue.Text);
            } catch { }
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            loadedLogic = new CombatLogic();
            LoadEntries();
        }

        private void LoadEntries()
        {
            listboxCombatActions.Items.Clear();
            foreach (CombatLogicEntry entry in loadedLogic.combatLogicEntries)
                listboxCombatActions.Items.Add(entry);
            prio = listboxCombatActions.Items.Count;
        }

        private void ComboboxActionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((CombatLogicEntry)listboxCombatActions.SelectedItem) != null)
                ((CombatLogicEntry)listboxCombatActions.SelectedItem).ActionType = (CombatActionType)comboboxActionType.SelectedItem;
        }
    }
}

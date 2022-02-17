using Stavadlo22.UserControls;

namespace Stavadlo22.Infrastructure
{
    /// <summary>
    /// Obsahuje metody pre zistenie, ci sa moze zmenit smer nastavenia vymeny-vyhybky
    /// </summary>
    class TrainSwitchManager
    {
        //TODO: pre BaseTrainSwitch pridat metodu SwitchChange_IsEnabled(...)
        /// <summary>
        /// podla aktualneho stavu vymeny testuje ci sa moze urobit zmena smeru vymeny;
        /// ak vymena nie je mozna, vypise odkaz do spodneho riadku;
        /// </summary>
        /// <param name="clickedSwitch">vymena na ktoru sa kliklo</param>
        /// <returns></returns>
        public static bool TrainSwitchChange_IsEnabled(BaseTrainSwitchControl clickedSwitch)
        {
            BaseTrainSwitchControl.SWITCH_STATE currentState = clickedSwitch.GetActualSwitchState();

            bool result = false;
            string message = string.Empty;
            switch (currentState)
            {
                case BaseTrainSwitchControl.SWITCH_STATE.NONE:
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.NORMAL:
                    //zmena smeru sa moze vykonat
                    return true;

                case BaseTrainSwitchControl.SWITCH_STATE.UVOLNENA_IZOLACIA:
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche, uvoľnená izolacia!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.OBSADENY_USEK:
                    message = "Výmeny: koľajový úsek " + clickedSwitch.Name + " je obsadený!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.VLAKOVA_CESTA:
                    message = "Výmeny: koľajový úsek " + clickedSwitch.Name + " je obsadený!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.POSUNOVA_CESTA:
                    message = "Výmeny: koľajový úsek " + clickedSwitch.Name + " je obsadený!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.VYHYBKA_V_MANIPULACII:
                    message = "Výmeny: " + clickedSwitch.Name + " sa prestavuje!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.VYLUKA_PRESTAVENIA:
                    message = "Výmeny: " + clickedSwitch.Name + " je vo výluke prestavenia!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.UPLNA_VYLUKA:
                    message = "Výmeny: " + clickedSwitch.Name + " je v úplnej výluke!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.STRATA_DOHLIADANIA://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.NADPRUD://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;
                case BaseTrainSwitchControl.SWITCH_STATE.PORUCHA_PRESTAVENIA://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.ROZREZ://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;

                default:
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;
            }

            //StavadloFactory.WriteRight(message, Enums.INFO_MODE.ERROR);
            //App.MessageWriter.WriteRight(message, Enums.INFO_MODE.ERROR);
            return result;
        }


        public static bool TrainSwitchChange_IsEnabled(BaseTrainSwitchControl clickedSwitch, out string message)
        {
            BaseTrainSwitchControl.SWITCH_STATE currentState = clickedSwitch.GetActualSwitchState();

            bool result = false;
            message = string.Empty;
            switch (currentState)
            {
                case BaseTrainSwitchControl.SWITCH_STATE.NONE:
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.NORMAL:
                    //zmena smeru sa moze vykonat
                    return true;

                case BaseTrainSwitchControl.SWITCH_STATE.UVOLNENA_IZOLACIA:
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche, uvoľnená izolacia!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.OBSADENY_USEK:
                    message = "Výmeny: koľajový úsek " + clickedSwitch.Name + " je obsadený!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.VLAKOVA_CESTA:
                    message = "Výmeny: koľajový úsek " + clickedSwitch.Name + " je obsadený!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.POSUNOVA_CESTA:
                    message = "Výmeny: koľajový úsek " + clickedSwitch.Name + " je obsadený!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.VYHYBKA_V_MANIPULACII:
                    message = "Výmeny: " + clickedSwitch.Name + " sa prestavuje!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.VYLUKA_PRESTAVENIA:
                    message = "Výmeny: " + clickedSwitch.Name + " je vo výluke prestavenia!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.UPLNA_VYLUKA:
                    message = "Výmeny: " + clickedSwitch.Name + " je v úplnej výluke!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.STRATA_DOHLIADANIA://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.NADPRUD://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;
                case BaseTrainSwitchControl.SWITCH_STATE.PORUCHA_PRESTAVENIA://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;

                case BaseTrainSwitchControl.SWITCH_STATE.ROZREZ://porucha
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;

                default:
                    message = "Výmeny: " + clickedSwitch.Name + " je v poruche!";
                    result = false;
                    break;
            }

            return result;
        }
    }
}

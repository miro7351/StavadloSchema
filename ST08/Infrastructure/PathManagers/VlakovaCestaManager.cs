using System;
using System.Windows;
using System.Windows.Shapes;
using System.Collections.Generic;

using SC = Stavadlo22.UserControls.BaseTrainSwitchControl;
using Stavadlo22.Data;
using Stavadlo22.UserControls;
using Stavadlo22.Infrastructure.Enums;
using Stavadlo22.Infrastructure.PathHelperPA;



namespace Stavadlo22.Infrastructure.PathManagers
{
    //MH: februar 2019
    //zalozena na kode pre TrainPathManger, aby sa nemiesal kod pre posunovu cestu a vlakovu cestu,
    //urobil som samostatne triedy PosunovaCestaManager a VlakovaCestaManager

    //Pozri kod v MessageManager.cs

    public class VlakovaCestaManager //instancia bude vytvorena v StavadloModel
    {
        StavadloModel stavadloModel;

        public VlakovaCestaManager(StavadloModel stavadloModel)
        {
            this.stavadloModel = stavadloModel;
            
            VlakCestaStav = PATH_CONSTRUCTION_STATE.START;
            vlakCestaVariantListFE = new List<FrameworkElement>();

            //event odpaleny po prijme spravy OK po vybere zaciatku vlakovej cesty
            stavadloModel.appEventsInvoker.UserSelStartPathVcestaValidEventFE += AppEventInvoker_UserSelStartPath_Vcesta_IsValidEventFE;

            //pre cesty s variantom
            //event odpaleny v MessageManager po dobrom vybere koncoveho useku vlakovej cesty, cesta este nie je vyfarbena,
            //alebo po dobrom vybere koncoveho useku vyluky/zrusenia vyluky posunovej alebo vlakovej cesty
            stavadloModel.appEventsInvoker.UserSelEndPathVcestaIsValidEventFE += AppEventInvoker_UserSelEndPath_Vcesta_IsValidEventFE;

            //pre cestu bez variantu
            //event odpaleny v MessageManager po prijme spravy OK po vybere konca posunovej cesty
            stavadloModel.appEventsInvoker.UserSelEndPathVcestaIsValidEvent_Variant0FE += AppEventInvoker_UserSelEndPathVcestaIsValidEvent_Variant0;

            //handler pre event odpaleny v MessageManageri ak sa vybral usek, ktory jednoznacne urcuje cestu, ak existuje viac variantov
            //handler pre event na konci stavania posunovej/vlakovej cesty, alebo na konci vyluky posun./vlakovej cesty
            stavadloModel.appEventsInvoker.TrainPathIsValidEvent += AppEventInvoker_TrainPathIsValidEvent;

            //eventy odpalene pri spracovani tlg.141 pozri UC_StavadloViewModel: NastavUsek, NastavVymenu
            //TOTO : MH zaremovane 20.02.2019 uz netreba????
            //stavadloModel.appEventsInvoker.NastavUsekSucastCesty_Event += AppEventInvoker_NastavUsekSucastCesty_Event;
            //stavadloModel.appEventsInvoker.NastavVymenuSucastCesty_Event += AppEventInvoker_NastavVymenuSucastCesty_Event;

            if (stavadloModel.WorkMode)
            {
                stavadloModel.communicator.TlgMessage151Recieved += TlgMessage151Recieved;
            }
            else//PlayMode rezim
            {
                //TODO: dokoncit!!!
                //stavadloModel.PMeventDriver.PMtlg151ReceivedEvent += PMeventDriver_PMtlg151Received;
            }
        }//ctor

        
        #region ---FIELDS/PROPERTIES-----
   
         /*PATH_CONSTRUCTION_TYPE: NONE, READY, START, END, VARIANT */

        public PATH_CONSTRUCTION_STATE VlakCestaStav; //stav vytvaranej vlakovej cesty
        
        int numberOfVariantsLocal;
        int NumberOfVariants; //pocet variantov pri stavani cesty, vyluke/ruseni vyluky cesty

        string semaforPrePovolenieCesty;

        /// <summary>
        /// Path - usek ktory je oznaceny ako zaciatok posunovej cesty
        /// </summary>
        //public Path vlakCestaZaciatokFE;
        public FrameworkElement vlakCestaZaciatokFE;


        /// <summary>
        /// Path - usek ktory je oznaceny ako koniec posunovej cesty
        /// </summary>
        public FrameworkElement vlakCestaKoniecFE;

        /// <summary>
        /// zoznam usekov, alebo vymen, ktore boli vybrate pre varianty posunovej cesty
        /// </summary>
        public List<FrameworkElement> vlakCestaVariantListFE;//


        /// <summary>
        /// Priznak ze povolenie cesty je povolene, nastavuje sa na true po prijme telegramu o skonceni stavania cesty;
        /// </summary>
        public string SemaforPrePovolenieCesty => semaforPrePovolenieCesty;

        bool povolenieCestyIsEnabled;
        public bool PovolenieCestyIsEnabled => povolenieCestyIsEnabled;

        #endregion  -----FIELDS/PROPERTIES-------

        #region -----Event Handlers --------


        /// <summary>
        /// handler pre event, ak bol dobre vybraty pociatocny usek vlakovej cesty;
        /// nastavi  VlakCestaStav = PATH_CONSTRUCTION.END;
        /// </summary>
        /// <param name="selectedPathName"></param>
        void AppEventInvoker_UserSelStartPath_Vcesta_IsValidEventFE(FrameworkElement el)
        {
            VlakCestaStav = PATH_CONSTRUCTION_STATE.END;
            vlakCestaZaciatokFE = el;
            povolenieCestyIsEnabled = false;
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (el is Path)
                {
                    (el as Path).SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
                }
            }));
        }

       
        /// <summary>
        /// handler pre event ak sa prijal tlg.121:Vcesta:Oznacte jeden z x variantov vlakovej cesty XXX-YYYY
        /// alebo tlg.121:Výluka vlakovej cesty: Označte jeden z x variantov vlakovej cesty XXXX-YYYY
        /// nastavi VlakCestaStav = PATH_CONSTRUCTION.VARIANT;
        /// len pre cestu s variantom;
        /// </summary>
        /// <param name="selectedPath"></param>
        void AppEventInvoker_UserSelEndPath_Vcesta_IsValidEventFE(FrameworkElement el)
        {
            VlakCestaStav = PATH_CONSTRUCTION_STATE.VARIANT;
            vlakCestaKoniecFE = el;
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if( el is Path)
                (el as Path).SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
            }));
        }

      
        /// <summary>
        /// handler pre event ak sa prijal telegram c. 151, VariantCesty;
        /// oznaceny prvok stavadla, usek alebo vymenu prida do zoznamu vlakCestaVariantListFE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TlgMessage151Recieved(object sender, Communication.EventArgs3 e)
        {
            numberOfVariantsLocal = e.Variant.var_number;
            string elementName = e.Variant.signalName;//Ak signalName.Length>0 je to platna cesta
            
            //POZN:stavia sa vlakova cesta, alebo sa robi vyluka vlakovej cesty a treba zadat variant cesty
            if ((stavadloModel.GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.VCESTA ||
                stavadloModel.GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.VYLUKA_VLAKOVEJ_CESTY
               ) &&
                 (numberOfVariantsLocal > 1) &&                        //pocet variantov cesty je vacsi ako 1
                 (NumberOfVariants > numberOfVariantsLocal) &&         //aby sme tu neprisli pri zadani konca cesty
                 (stavadloModel.ClickedElement != null) &&            //prvok stavadla na ktory bol urobeny klik
                 VlakCestaStavIsVariant()  //stavanie vlakovej cesty je v mode Variant
                )
            {
                var clickedElement = stavadloModel.ClickedElement;
                bool isSwitch = clickedElement is BaseTrainSwitchControl;
                //------------------------

                
                if (VlakCestaStavIsVariant())//stav budovania vlakovej cesty je: PATH_CONSTRUCTION_TYPE.VARIANT 
                {
                    if (clickedElement is BaseTrainSwitchControl)//klik na vymenu
                    {
                        BaseTrainSwitchControl btsc = sender as BaseTrainSwitchControl;

                        if (btsc.SwitchState != SC.SWITCH_STATE.PORUCHA_PRESTAVENIA &&
                           btsc.SwitchState != SC.SWITCH_STATE.ROZREZ &&
                           btsc.SwitchState != SC.SWITCH_STATE.NADPRUD &&
                           btsc.SwitchState != SC.SWITCH_STATE.STRATA_DOHLIADANIA &&
                           btsc.SwitchState != SC.SWITCH_STATE.UPLNA_VYLUKA)//ak vymena ma pouchu, nemoze sa oznacit ako variant vlakovej cesty
                        {
                            vlakCestaVariantListFE.Add((FrameworkElement)clickedElement);

                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                btsc.SetValue(BaseTrainSwitchControl.SwitchStateProperty, SC.SWITCH_STATE.VARIANT);
                            }));
                           
                        }
                    }
                    else if (clickedElement is Path)//klik na usek
                    {
                        vlakCestaVariantListFE.Add((FrameworkElement)clickedElement);
                        App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        {
                            (clickedElement as Path).SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
                        }));
                    }
                }
            }
            NumberOfVariants = e.Variant.var_number;

            //MH: pridane 28.03.2019
            if ((stavadloModel.GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.VCESTA) && VlakCestaStavIsEnd())
            {
                if (numberOfVariantsLocal == 1)//e.Variant.signalName je Name semaforu, na ktory sa presunie kurzor aby operator povolil posun. cestu 
                {
                    semaforPrePovolenieCesty = e.Variant.signalName;
                    povolenieCestyIsEnabled = true;
                }
            }
        }//TlgMessage151Recieved


        /// <summary>
        /// handler pre event na konci stavania vlakovej cesty, alebo na konci vyluky vlakovej cesty
        /// </summary>
        void AppEventInvoker_TrainPathIsValidEvent()
        {
            ZrusOznacenieStavaniaVlakovejCesty();
        }


        /// <summary>
        /// handler pre event ak sa prijal tlg.121:OK po vybere konca vlakovej cesty;
        /// len pre cestu bez variantu;
        /// </summary>
        /// <param name="selPathName"></param>
        void AppEventInvoker_UserSelEndPathVcestaIsValidEvent_Variant0(object selPathName)
        {
            ZrusOznacenieStavaniaVlakovejCesty();
        }

        #endregion -----Event Handlers------



        #region ----- FUNKCIE ------

        /*Zrusenie vlakovej cesty:
            * -operator postavil celu vlakovu cestu, vlak presiel, a dispecer ju chce zrusit
            * -operator oznacil zaciatok vlakovej cesty, koniec este nie, ale sa pomylil, napr. klikol na nespravny usek
            *  a potrebuje zrusit oznacenie zaciatku vlakovej cesty
            * 
            * Zaciatok vlakovej cesty moze byt usek, ktory nepatri vymene, alebo usek ktory patri vymene.
            */

        public void ZrusOznacenieStavaniaVlakovejCesty()
        {
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (vlakCestaZaciatokFE != null)
                {
                    if (vlakCestaZaciatokFE is Path)
                    {
#if DEBUG
                        Path selPath = vlakCestaZaciatokFE as Path;
                        string pathName = selPath.Name;
                        PATH_MODE currentMode = (PATH_MODE)vlakCestaZaciatokFE.GetValue(PathHelper.ModeProperty);
#endif
                        PATH_MODE previousMode = (PATH_MODE)vlakCestaZaciatokFE.GetValue(PathHelper.PreviousModeProperty);
                        vlakCestaZaciatokFE.SetValue(PathHelper.ModeProperty, previousMode);
                    }


                    if (VlakCestaStav == PATH_CONSTRUCTION_STATE.END || VlakCestaStav == PATH_CONSTRUCTION_STATE.VARIANT)
                    {
                        //tu este moze byt vlakCestaKoniecFE = null, ale uz bol oznaceny aj koniec posunovej cesty
                        if (vlakCestaKoniecFE != null && vlakCestaKoniecFE.Name != vlakCestaZaciatokFE.Name)
                        {
                            Path selPath = vlakCestaKoniecFE as Path;
                            PATH_MODE currentMode = (PATH_MODE)selPath.GetValue(PathHelper.ModeProperty);
                            PATH_MODE previousMode = (PATH_MODE)selPath.GetValue(PathHelper.PreviousModeProperty);
                            if ((currentMode != PATH_MODE.VLAKOVA_CESTA) && (currentMode != PATH_MODE.START_END_PATH_VLAK))
                                selPath.SetValue(PathHelper.ModeProperty, previousMode);
                        }
                        if (vlakCestaVariantListFE != null)
                        {
                            foreach (FrameworkElement fe in vlakCestaVariantListFE)
                            {
                                if (fe is Path)
                                {

                                    Path selPath = fe as Path;
                                    string pathName = selPath.Name;
                                    PATH_MODE currentMode = (PATH_MODE)selPath.GetValue(PathHelper.ModeProperty);

                                    PATH_MODE previousMode = (PATH_MODE)selPath.GetValue(PathHelper.PreviousModeProperty);
                                    if ((currentMode != PATH_MODE.VLAKOVA_CESTA) && (currentMode != PATH_MODE.START_END_PATH_POSUN))
                                        selPath.SetValue(PathHelper.ModeProperty, previousMode);
                                }
                                else if (fe is BaseTrainSwitchControl)
                                {
                                    //TODO: OTESTOVAT
                                    BaseTrainSwitchControl swichControl = fe as BaseTrainSwitchControl;
                                    SC.SWITCH_STATE previousState = (SC.SWITCH_STATE)swichControl.GetValue(BaseTrainSwitchControl.PreviousSwitchStateProperty);
                                    swichControl.SetValue(BaseTrainSwitchControl.SwitchStateProperty, previousState);
                                }
                            }
                        }//if(vlakCestaVariantListFE != null)
                    }
                }

                VlakCestaStav = PATH_CONSTRUCTION_STATE.START;
                vlakCestaKoniecFE = null;
                vlakCestaZaciatokFE = null;

                vlakCestaVariantListFE.Clear();
                NumberOfVariants = 0;
            }));
        }//ZrusOznacenieStavaniaVlakovejCesty


        public bool VlakCestaStavIsStart()
        {
            return VlakCestaStav == PATH_CONSTRUCTION_STATE.START;
        }


        public bool VlakovaCestaStavIsVariant
        {
            get { return VlakCestaStav == PATH_CONSTRUCTION_STATE.START; }
        }

        public bool VlakCestaStavIsEnd()
        {
            return VlakCestaStav == PATH_CONSTRUCTION_STATE.END;
        }
        public bool VlakCestaStavIsVariant()
        {
            return VlakCestaStav == PATH_CONSTRUCTION_STATE.VARIANT;
        }

        public bool VlakCestaStavIsReady()
        {
            return VlakCestaStav == PATH_CONSTRUCTION_STATE.READY;
        }

        /// <summary>
        ///  vrati uplny nazov cesty podla vlakCestaZaciatok.Name, vlakCestaKoniec.Name;
        ///  pouziva sa pri vypise oznamov pre cesty s variantmi
        /// </summary>
        /// <returns></returns>
        public string GetCurrentFullPathName_Vcesta2()
        {
            if ((vlakCestaZaciatokFE != null) && (vlakCestaKoniecFE != null))
                return string.Format("{0}-{1}", vlakCestaZaciatokFE.Name, vlakCestaKoniecFE.Name);
            else
                return string.Empty;
        }


        /// <summary>
        /// vrati kod vlakovej cesty, default=x
        /// </summary>
        /// <returns></returns>
        public char GetVlakCestaKod()
        {
            char kod = 'x';
            switch (VlakCestaStav)     //stav v ktorom sa nachadza stavanie vlakovej cesty
            {
                case PATH_CONSTRUCTION_STATE.START:
                    kod = 'z';          //nastavenie kodu na zaciatok
                    break;
                case PATH_CONSTRUCTION_STATE.END:
                    kod = 'k';
                    break;
                case PATH_CONSTRUCTION_STATE.VARIANT:
                    kod = 'v';
                    break;
                default:
                    break;
            }
            return kod;
        }

       

        #endregion -- FUNKCIE ------

    }
}

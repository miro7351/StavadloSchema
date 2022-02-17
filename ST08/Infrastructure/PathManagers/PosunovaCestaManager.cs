using System;
using System.Windows;
using System.Windows.Shapes;
using System.Collections.Generic;

using Stavadlo22.Data;
using Stavadlo22.UserControls;
using Stavadlo22.Data.Telegrams;
using Stavadlo22.Infrastructure.Enums;
using Stavadlo22.Infrastructure.Communication;
using Stavadlo22.Infrastructure.PathHelperPA;
using SC = Stavadlo22.UserControls.BaseTrainSwitchControl;
using log4net;

namespace Stavadlo22.Infrastructure.PathManagers
{

    //MH: februar 2019
    //zalozena na kode pre TrainPathManger, aby sa nemiesal kod pre posunovu cestu a vlakovu cestu,
    //urobil som samostatne triedy PosunovaCestaManager a VlakovaCestaManager

    //Pozri kod v MessageManager.cs

    public class PosunovaCestaManager    //instancia bude vytvorena v StavadloModel
    {
        StavadloModel stavadloModel;
        ILog Log;
        public PosunovaCestaManager(StavadloModel stavadloModel)
        {
            this.stavadloModel = stavadloModel;
            
            Log = StavadloGlobalData.Instance.Log;

            PosunCestaStav = PATH_CONSTRUCTION_STATE.START;
            posunCestaVariantListFE = new List<FrameworkElement>();
            semaforPrePovolenieCesty = string.Empty;
            povolenieCestyIsEnabled = false;

            //event odpaleny v MessageManager po prijme spravy OK po vybere zaciatku posunovej cesty
            //
            //stavadloModel.appEventsInvoker.UserSelStartPathPcestaValidEventFE += AppEventInvoker_UserSelStartPath_Pcesta_IsValidEventFE; 
            stavadloModel.appEventsInvoker.UserSelStartPathPcestaValidEventFE2 += AppEventInvoker_UserSelStartPath_Pcesta_IsValidEventFE2;

            //pre cesty s variantom
            //event odpaleny v MessageManager po dobrom vybere koncoveho useku posunovej/vlakovej cesty, cesta este nie je vyfarbena,
            //alebo po dobrom vybere koncoveho useku vyluky/zrusenia vyluky posunovej alebo vlakovej cesty
            stavadloModel.appEventsInvoker.UserSelEndPathPcestaIsValidEventFE += AppEventInvoker_UserSelEndPath_Pcesta_IsValidEventFE;

            //pre cestu bez variantu
            //event odpaleny v MessageManager po prijme spravy OK po vybere konca posunovej cesty
            //stavadloModel.appEventsInvoker.UserSelEndPathPcestaIsValidEvent_Variant0 += AppEventInvoker_UserSelEndPathPcestaIsValidEvent_Variant0;//MH-kontrola OK
            //stavadloModel.appEventsInvoker.UserSelEndPathPcestaIsValidEvent_Variant0FE += AppEventInvoker_UserSelEndPathPcestaIsValidEvent_Variant0;
         
            stavadloModel.appEventsInvoker.UserSelEndPathPcestaIsValidEvent_Variant0FE2 += AppEventInvoker_UserSelEndPathPcestaIsValidEvent_Variant0;//MH 22.03.2019

            //handler pre event odpaleny v MessageManageri ak sa vybral usek, ktory jednoznacne urcuje cestu, ak existuje viac variantov
            //handler pre event na konci stavania posunovej/vlakovej cesty, alebo na konci vyluky posun./vlakovej cesty
            stavadloModel.appEventsInvoker.TrainPathIsValidEvent += AppEventInvoker_TrainPathIsValidEvent;//MH-kontrola OK

            //eventy odpalene pri spracovani tlg.141 pozri UC_StavadloViewModel: NastavUsek, NastavVymenu
            //TOTO: MH zaremovane 20.02.2019 uz netreba????
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



        /*PATH_CONSTRUCTION_TYPE: NONE, READY, START, END, VARIANT */
        
        public PATH_CONSTRUCTION_STATE PosunCestaStav;//stav vytvaranie posunovej cesty

        int numberOfVariantsLocal;
        int NumberOfVariants; //pocet variantov pri stavani cesty, vyluke/ruseni vyluky cesty

        string semaforPrePovolenieCesty;

        /// <summary>
        /// priznak ci sa moze povolitCesta
        /// </summary>
        bool povolenieCestyIsEnabled;

        #region ---PROPERTIES---

        /// <summary>
        /// Path - usek ktory je oznaceny ako zaciatok posunovej cesty
        /// </summary>
        //public Path posunCestaZaciatokFE;

        public FrameworkElement posunCestaZaciatokFE;//MH: 23.03.2019

        /// <summary>
        /// Path - usek ktory je oznaceny ako koniec posunovej cesty
        /// </summary>
       // public Path posunCestaKoniecFE;

        public FrameworkElement posunCestaKoniecFE;//MH: 23.03.2019

        /// <summary>
        /// zoznam usekov, alebo vymen, ktore boli vybrate pre varianty posunovej cesty
        /// </summary>
        public List<FrameworkElement> posunCestaVariantListFE;//

        /// <summary>
        /// Priznak ze povolenie cesty je povolene, nastavuje sa na true po prijme telegramu o skonceni stavania cesty;
        /// </summary>
        public string SemaforPrePovolenieCesty => semaforPrePovolenieCesty;

        public bool PovolenieCestyIsEnabled => povolenieCestyIsEnabled;

        #endregion  -----PROPERTIES-------



        #region ----Event handlers ------

        /*
        //MH modifikacia 11.01.2019------------
        /// <summary>
        /// Handler pre event, po prijme spravy OK po vybere zaciatku posunovej cesty
        /// ak bol dobre vybraty pociatocny usek posunovej cesty;
        /// Vyfarbi pociatocny usek cesty (biely ramik okolo path) a nastavi PosunCestaStav = PATH_CONSTRUCTION_STATE.END;
        /// </summary>
        /// <param name="selectedPath"></param>
        void AppEventInvoker_UserSelStartPath_Pcesta_IsValidEventFE(Path selectedPath)   //event sa odpaluje v MessageManager.cs
        {
            PosunCestaStav = PATH_CONSTRUCTION_STATE.END;
            //nastavenie-vyfarbenie useku-path na ktory sa kliklo pri vybere zaciatku Posunovej cesty
            //posunCestaZaciatok = new SelectedPath(selectedPath.Name, (PATH_MODE)selectedPath.GetValue(PathHelper.ModeProperty) );
            //selectedPath.SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
            //MH 12.01.2019
            posunCestaZaciatokFE = selectedPath;
            //posunCestaVariantListFE.Add(posunCestaZaciatokFE);

            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
#if DEBUG
                string pathName = selectedPath.Name;//MH: len na test na otestovanie nacitania ModeProperty a PreviousModeProperty
                var v2 = (PATH_MODE)selectedPath.GetValue(PathHelper.ModeProperty);
                var v1 = (PATH_MODE)selectedPath.GetValue(PathHelper.PreviousModeProperty); 
#endif
                selectedPath.SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);//attached property PathHelper.ModeProperty
            }) );
        }
        */

        //MH modifikacia 21.03.2019------------
        /// <summary>
        /// Handler pre event, po prijme spravy OK po vybere zaciatku posunovej cesty
        /// ak bol dobre vybraty pociatocny usek posunovej cesty;
        /// Vyfarbi pociatocny usek cesty (biely ramik okolo path) a nastavi PosunCestaStav = PATH_CONSTRUCTION_STATE.END;
        /// </summary>
        /// <param name="selectedPath"></param>
        void AppEventInvoker_UserSelStartPath_Pcesta_IsValidEventFE2( FrameworkElement el)   //event sa odpaluje v MessageManager.cs
        {
            PosunCestaStav = PATH_CONSTRUCTION_STATE.END;
//#if DEBUG
//            log?.CustomInfo($"PosunCestaManager-AppEventInvoker_UserSelStartPath_Pcesta_IsValidEventFE2 PosunCestaStav={PosunCestaStav}");
//            System.Diagnostics.Debug.WriteLine($"PosunCestaManager-AppEventInvoker_UserSelStartPath_Pcesta_IsValidEventFE2 PosunCestaStav={PosunCestaStav} {DateTime.Now:HH:mm:ss.fff}");
//#endif
            povolenieCestyIsEnabled = false; //az po prijme tlg. 151 sa nastavi na true
            posunCestaZaciatokFE = el;
            //nastavenie-vyfarbenie useku-path na ktory sa kliklo pri vybere zaciatku Posunovej cesty
            if (el is Path)
            {
                Path selectedPath = el as Path;

                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                {
//#if DEBUG
//                    string pathName = selectedPath.Name;//MH: len na test na otestovanie nacitania ModeProperty a PreviousModeProperty
//                    var v2 = (PATH_MODE)selectedPath.GetValue(PathHelper.ModeProperty);
//                    var v1 = (PATH_MODE)selectedPath.GetValue(PathHelper.PreviousModeProperty);
//#endif
                selectedPath.SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);//attached property PathHelper.ModeProperty
                }));
            }
            else if( el is Semafor1Control) //klik na semafor, ktory moze byt ako zaciatok Posunovej cesty
            {
                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                {
                    Semafor1Control sem1 = el as Semafor1Control;
                    if( sem1.StartEndEnabled) //semafor moze byt zaciatkom, alebo koncom posunovej cesty
                    {
                        sem1.LightMode = SEMAFOR1_MODE.START_END_PART_OF_WAY;
                        sem1.CanSemaphoreResetToNormal = true;
                    }
                }));
            }
        }

        /*
        /// <summary>
        /// handler pre event odpaleny v UC_StavadloViewModel.cs vo funkcii NastavUsek;
        /// ak usek je sucastou cesty, potom ho nastavi ako sucast cesty pri spracovani telegramu c.141
        /// </summary>
        /// <param name="pathName"></param>
        void AppEventInvoker_NastavUsekSucastCesty_Event(Path myPath)
        {

            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {

                PATH_MODE actMode = (PATH_MODE)myPath.GetValue(PathHelper.ModeProperty);
                string pathName = myPath.Name;

                if (posunCestaZaciatokFE != null && string.Equals(myPath.Name, posunCestaZaciatokFE.Name))
                {
                    myPath.SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
                }
                if (posunCestaKoniecFE != null && string.Equals(myPath.Name, posunCestaKoniecFE.Name))
                {
                    myPath.SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
                }

                foreach (var element in posunCestaVariantListFE)
                {
                    if (element is Path)
                    {
                        FrameworkElement pathVariant = element as FrameworkElement;
                        if (string.Equals(myPath.Name, pathVariant.Name))
                        {
                            //SetStavanieCesty(pathModeProperty);
                            myPath.SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
                        }
                    }
                }
            }));
        }//AppEventInvoker_NastavUsekSucastCesty_Event
        */

        //MH uprava: 11.01.2019 ------------
        /// <summary>
        /// handler pre event ak sa prijal tlg.121:Pcesta:Oznacte jeden z x variantov posunovej cesty XXX-YYYY;
        /// alebo tlg.121:Výluka posunovej cesty: Označte jeden z x variantov posunovej cesty XXXX-YYYY
        /// nastavi PosunCestaStav = PATH_CONSTRUCTION.VARIANT;
        /// len pre cestu s variantom;
        /// </summary>
        /// <param name="selectedPathName"></param>
        void AppEventInvoker_UserSelEndPath_Pcesta_IsValidEventFE(FrameworkElement el)  //event sa odpaluje v MessageManageri
        {
            PosunCestaStav = PATH_CONSTRUCTION_STATE.VARIANT;
//#if DEBUG
//            log?.CustomInfo("PosunCestaManager- AppEventInvoker_UserSelEndPath_Pcesta_IsValidEventFE {PosunCestaStav}");
//#endif           
            posunCestaKoniecFE = el;
           
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (el is Path)
                {
                    Path clickedPath = el as Path;
                    clickedPath.SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
                }
                if( el is Semafor1Control)
                {
                    Semafor1Control sem1 = el as Semafor1Control;
                    if( sem1.StartEndEnabled)
                    {
                        sem1.LightMode = SEMAFOR1_MODE.START_END_PART_OF_WAY;
                        sem1.CanSemaphoreResetToNormal = true;
                    }
                }
            }));
        }

        //-----------------------------------

        /// <summary>
        /// handler pre event ak sa prijal telegram c. 151, VariantCesty;
        /// oznaceny prvok stavadla, usek alebo vymenu prida do zoznamu posunCestaVariantListFE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TlgMessage151Recieved(object sender, Communication.EventArgs3 e)
        {
            numberOfVariantsLocal = e.Variant.var_number;
            string elementName = e.Variant.signalName;//Ak signalName.Length>0 je to platna cesta
            
            //POZN:stavia sa posunova cesta, alebo sa robi vyluka posunovej a treba zadat variant cesty
            if ((stavadloModel.GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.PCESTA ||
                stavadloModel.GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.VYLUKA_POSUNOVEJ_CESTY 
                 ) &&
                 (numberOfVariantsLocal > 1) &&                        //pocet variantov cesty je vacsi ako 1
                 (NumberOfVariants > numberOfVariantsLocal) &&         //aby sme tu neprisli pri zadani konca cesty
                 (stavadloModel.ClickedElement!=null)  &&            //prvok stavadla na ktory bol urobeny klik
                 PosunCestaStavIsVariant()  //stavanie cesty je v mode Variant
                )
            {

                var clickedElement = stavadloModel.ClickedElement;// sender;// StavadloGlobalData.Instance.ClickedElement;
                bool isSwitch = clickedElement is BaseTrainSwitchControl;
               
               
                if (PosunCestaStavIsVariant())//stav budovania posunovej cesty je: PATH_CONSTRUCTION_TYPE.VARIANT 
                {
                    if (clickedElement is BaseTrainSwitchControl)//klik na vymenu
                    {
                        BaseTrainSwitchControl btsc = clickedElement as BaseTrainSwitchControl;

                        if( (btsc.SwitchState != SC.SWITCH_STATE.PORUCHA_PRESTAVENIA) &&
                            (btsc.SwitchState != SC.SWITCH_STATE.ROZREZ) &&
                            (btsc.SwitchState != SC.SWITCH_STATE.NADPRUD) &&
                            (btsc.SwitchState != SC.SWITCH_STATE.STRATA_DOHLIADANIA) &&
                            (btsc.SwitchState != SC.SWITCH_STATE.UPLNA_VYLUKA)   )//ak vymena ma pouchu, nemoze sa oznacit ako variant posunovej cesty
                        {
                            posunCestaVariantListFE.Add((FrameworkElement)clickedElement);
                            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                            {
                                btsc.SetValue(BaseTrainSwitchControl.SwitchStateProperty, SC.SWITCH_STATE.VARIANT);
                            }));
                        }
                    }
                    else if( clickedElement is Path)//klik na usek
                    { 
                        posunCestaVariantListFE.Add((FrameworkElement)clickedElement);
                        App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                        {
                            (clickedElement as Path).SetValue(PathHelper.ModeProperty, PATH_MODE.START_END_PATH);
                        }));
                    }
                }
            }
            NumberOfVariants = e.Variant.var_number;

            //MH: pridane 28.03.2019
            if( (stavadloModel.GlobalData.CurrentMenuMode == CURRENT_MENU_MODE.PCESTA) && PosunCestaStavIsEnd() )
            {
                if(numberOfVariantsLocal==1)//e.Variant.signalName je Name semaforu, na ktory sa presunie kurzor aby operator povolil posun. cestu 
                {
                    semaforPrePovolenieCesty = e.Variant.signalName;
                    povolenieCestyIsEnabled = true;
                }
            }

        }//TlgMessage151Recieved


        /*
        /// <summary>
        /// handler pre event odpaleny controlom <see cref="BaseTrainSwitchControl"/>  vo funkcii OnTsStatusChanged;
        /// ak vymena je sucast cesty, potom pri spracovani tlg.141 sa oznaci ako sucast cesty;
        /// spusta sa len pre vymeny, ktore nemaju poruchu!!!
        /// </summary>
        /// <param name="switchName"></param>
        //void AppEventInvoker_NastavVymenuSucastCesty_Event(string switchName)
        void AppEventInvoker_NastavVymenuSucastCesty_Event(BaseTrainSwitchControl btsControl)
        {
            string switchName = btsControl.Name;

            bool switchIsInVlakCesta = false;
            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                SC.SWITCH_STATE actState = btsControl.SwitchState;
               
                if (!switchIsInVlakCesta)
                {
                    foreach (FrameworkElement element in posunCestaVariantListFE)
                    {
                        if (element is BaseTrainSwitchControl)
                        {
                            BaseTrainSwitchControl switchVariantControl = element as BaseTrainSwitchControl;
                            //if (string.Equals(switchName, switchVariant.Name))
                            if( btsControl == switchVariantControl)
                            {
                                //statePropertyInfo.SetValue(stavadloElements, SC.SWITCH_STATE.VARIANT, null);
                                btsControl.SwitchState = SC.SWITCH_STATE.VARIANT;
                                //Pozri BaseTrainSwitchControl.cs metodu OnSwitchStateChanged, zabezpeci vyfarbenie okraju vymeny do stav variant
                                //switchVariant.Mode = actState;
                            }
                        }
                    }//foreach
                }
            }));
        }//AppEventInvoker_NastavVymenuSucastCesty_Event

        */
        /// <summary>
        /// handler pre event na konci stavania posunovej/vlakovej cesty, alebo na konci vyluky posun./vlakovej cesty
        /// </summary>
        void AppEventInvoker_TrainPathIsValidEvent()
        {
//#if DEBUG
//            log?.CustomInfo("PosunCestaManager- AppEventInvoker_TrainPathIsValidEvent");
//#endif
            ZrusOznacenieStavaniaPosunovejCesty();
        }

        /// <summary>
        /// handler pre event ak sa prijal tlg.121:OK po vybere konca posunovej cesty;
        /// len pre cestu bez variantu;
        /// </summary>
        /// <param name="selPathName"></param>
        void AppEventInvoker_UserSelEndPathPcestaIsValidEvent_Variant0(object el)
        {
//#if DEBUG
//            log?.CustomInfo("PosunCestaManager-AppEventInvoker_UserSelEndPathPcestaIsValidEvent_Variant0");
//#endif
            ZrusOznacenieStavaniaPosunovejCesty();
        }


        /// <summary>
        /// handler pre event, ktory sa odpaluje v PlayMode
        /// </summary>
        /// <param name="e"></param>
        void PMeventDriver_PMtlg151Received(EventArgs3 e)
        {
            VariantCesty variant = e.Variant;
            TlgMessage151Recieved(null, e);
        }

        #endregion--Event handlers ------





        #region ----FUNKCIE -----

            /* Zrusenie posunovej cesty:
             * Operator oznacil zaciatok posunovej cesty, koniec este nie, ale sa pomylil, napr. klikol na nespravny usek
             *   a potrebuje zrusit oznacenie zaciatku posunovej cesty pomocou kliku na stredne tlacidlo mysi, alebo klikne na nejaky menuitem, napr. O programe
             * 
             * Zaciatok posunovej cesty moze byt usek, ktory nepatri vymene, alebo usek ktory patri vymene??.
             */

        /// <summary>
        /// vrati do povodneho stavu prvky, ktore boli oznacene ako zaciatok, alebo koniec PCesty;
        /// nastavi PosunCestaStav = PATH_CONSTRUCTION_STATE.START;
        /// </summary>
        public void ZrusOznacenieStavaniaPosunovejCesty() //MH 22.03.2019; spusta sa po kliku na ktorykolvek MenuItem, spusta sa po skonceni postavenia PCesty!!!!!!!!!!! 
        {
//#if DEBUG
//            log?.CustomInfo("PosunCestaManager-ZrusOznacenieStavaniaPosunovejCesty-zaciatok");
//#endif
            povolenieCestyIsEnabled = false;

            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (posunCestaZaciatokFE != null) //bol uz vybraty usek pre zaciatok posunovej cesty
                {
                    if (posunCestaZaciatokFE is Path)  //pozn. nastavi sa attached property Mode pre usek, ktory je zaciatok posun. cesty do povodnej hodnoty, pred nastavenim ...
                    {
                        Path selPath = posunCestaZaciatokFE as Path;
                        PATH_MODE currentMode = (PATH_MODE)selPath.GetValue(PathHelper.ModeProperty);//len pre kontrolu
                        PATH_MODE previousMode = (PATH_MODE)selPath.GetValue(PathHelper.PreviousModeProperty);
                        selPath.SetValue(PathHelper.ModeProperty, previousMode);

                        //uz bol oznaceny zaciatok posunovej cesty, alebo sa uz vybera nejaky variant posunovej cesty
                        if (PosunCestaStav == PATH_CONSTRUCTION_STATE.END || PosunCestaStav == PATH_CONSTRUCTION_STATE.VARIANT)
                        {
                            if (posunCestaKoniecFE != null && (posunCestaKoniecFE.Name != posunCestaZaciatokFE.Name) )
                            {
                                //pozn. nastavi sa attached property Mode pre usek, ktory je koniec posun. cesty do povodnej hodnoty, pred nastavenim ...
                                //TODO: 12.01.2019 PathHelper ma DP PATH_MODE PreviousModeProperty, obsahuje hodnotu ModeProperty, ktora bola pred aktualnou hodnotou ModeProperty;
                                if (posunCestaKoniecFE is Path)
                                {
                                    selPath = posunCestaKoniecFE as Path;
                                    currentMode = (PATH_MODE)selPath.GetValue(PathHelper.ModeProperty);
                                    previousMode = (PATH_MODE)selPath.GetValue(PathHelper.PreviousModeProperty);
                                    if ((currentMode != PATH_MODE.POSUNOVA_CESTA) && (currentMode != PATH_MODE.START_END_PATH_POSUN))
                                        selPath.SetValue(PathHelper.ModeProperty, previousMode);
                                }
                                else if (posunCestaKoniecFE is Semafor1Control)
                                {
                                    Semafor1Control sem1 = posunCestaKoniecFE as Semafor1Control;
                                    SEMAFOR1_MODE semaforPreviousMode = sem1.PreviousLightMode;
                                    sem1.SetValue(Semafor1Control.LightModeProperty, semaforPreviousMode);
                                    //toto je malo zrozumitelne!!
                                   // (posunCestaKoniecFE as Semafor1Control).SetValue(Semafor1Control.LightModeProperty, (posunCestaKoniecFE as Semafor1Control).PreviousLightMode);
                                }
                            }
                            if (posunCestaVariantListFE != null)
                            {
                                foreach (FrameworkElement fe in posunCestaVariantListFE)
                                {
                                    if (fe is Path)
                                    {
                                        selPath = fe as Path;
                                        string pathName = selPath.Name;
                                        currentMode = (PATH_MODE)selPath.GetValue(PathHelper.ModeProperty);
                                        previousMode = (PATH_MODE)selPath.GetValue(PathHelper.PreviousModeProperty);
                                        if ((currentMode != PATH_MODE.POSUNOVA_CESTA) && (currentMode != PATH_MODE.START_END_PATH_POSUN))
                                            selPath.SetValue(PathHelper.ModeProperty, previousMode);

                                    }
                                    else if (fe is BaseTrainSwitchControl)
                                    {
                                        //TODO: OTESTOVAT
                                        BaseTrainSwitchControl swichControl = fe as BaseTrainSwitchControl;
                                        string controlName = swichControl.Name;
                                        SC.SWITCH_STATE previousState = (SC.SWITCH_STATE)swichControl.GetValue(BaseTrainSwitchControl.PreviousSwitchStateProperty);
                                        swichControl.SetValue(BaseTrainSwitchControl.SwitchStateProperty, previousState);
                                    }
                                }
                            }//if(posunCestaVariantList != null)
                        }// if (PosunCestaStav == PATH_CONSTRUCTION.END || PosunCestaStav == PATH_CONSTRUCTION.VARIANT)
                    }//posunCestaZaciatokFE is Path

                    if ( posunCestaZaciatokFE is Semafor1Control)
                    {
                        //pozn. nastavi DP property LightModeProperty pre semafor, ktory je zaciatok posun. cesty do povodnej hodnoty, pred nastavenim ...
                        Semafor1Control semafor = posunCestaZaciatokFE as Semafor1Control;
                        SEMAFOR1_MODE semaforPreviousMode = semafor.PreviousLightMode;
                        semafor.SetValue(Semafor1Control.LightModeProperty, semaforPreviousMode);

                        if (PosunCestaStav == PATH_CONSTRUCTION_STATE.END || PosunCestaStav == PATH_CONSTRUCTION_STATE.VARIANT)
                        {
                            PATH_MODE currentMode;
                            PATH_MODE previousMode;
                            if (posunCestaKoniecFE != null && ( posunCestaKoniecFE is Path) && (posunCestaKoniecFE.Name != posunCestaZaciatokFE.Name)  )
                            {
                                //pozn. nastavi sa attached property Mode pre usek, ktory je koniec posun. cesty do povodnej hodnoty, pred nastavenim ...
                                //TODO: 12.01.2019 PathHelper ma DP PATH_MODE PreviousModeProperty, obsahuje hodnotu ModeProperty, ktora bola pred aktualnou hodnotou ModeProperty;
                                if (posunCestaKoniecFE is Path)
                                {
                                    Path selPath = posunCestaKoniecFE as Path;
                                    currentMode = (PATH_MODE)selPath.GetValue(PathHelper.ModeProperty);
                                    previousMode = (PATH_MODE)selPath.GetValue(PathHelper.PreviousModeProperty);
                                    if ((currentMode != PATH_MODE.POSUNOVA_CESTA) && (currentMode != PATH_MODE.START_END_PATH_POSUN))
                                        selPath.SetValue(PathHelper.ModeProperty, previousMode);
                                }
                            }
                            if (posunCestaVariantListFE != null)
                            {
                                foreach (FrameworkElement fe in posunCestaVariantListFE)
                                {
                                    if (fe is Path)
                                    {
                                        Path selPath = fe as Path;
                                        string pathName = selPath.Name;
                                        currentMode = (PATH_MODE)selPath.GetValue(PathHelper.ModeProperty);
                                        previousMode = (PATH_MODE)selPath.GetValue(PathHelper.PreviousModeProperty);
                                        if ((currentMode != PATH_MODE.POSUNOVA_CESTA) && (currentMode != PATH_MODE.START_END_PATH_POSUN))
                                            selPath.SetValue(PathHelper.ModeProperty, previousMode);

                                    }
                                    else if (fe is BaseTrainSwitchControl)
                                    {
                                        //TODO: OTESTOVAT
                                        BaseTrainSwitchControl vymena = fe as BaseTrainSwitchControl;
                                        string controlName = vymena.Name;
                                        SC.SWITCH_STATE prevStateVymena = (SC.SWITCH_STATE)vymena.GetValue(BaseTrainSwitchControl.PreviousSwitchStateProperty);
                                        vymena.SetValue(BaseTrainSwitchControl.SwitchStateProperty, prevStateVymena);
                                    }
                                }
                            }//if(posunCestaVariantList != null)
                        }// if (PosunCestaStav == PATH_CONSTRUCTION.END || PosunCestaStav == PATH_CONSTRUCTION.VARIANT)
                    }//posunCestaZaciatokFE is Semafor1Control
                }//if (posunCestaZaciatok != null)

                PosunCestaStav = PATH_CONSTRUCTION_STATE.START;
//#if DEBUG
//                log?.CustomInfo($"PosunCestaManager-ZrusOznacenieStavaniaPosunovejCesty-koniec: PosunCestaStav={PosunCestaStav}");
//#endif
                posunCestaKoniecFE = null;
                posunCestaZaciatokFE = null;

                posunCestaVariantListFE.Clear();
                NumberOfVariants = 0;
            }));// App.Current.Dispatcher.BeginInvoke(
        }//ZrusOznacenieStavaniaPosunovejCesty


        public bool PosunCestaStavIsStart()
        {
            return PosunCestaStav == PATH_CONSTRUCTION_STATE.START;
        }

        public bool PosunCestaStavIsEnd()
        {
            return PosunCestaStav == PATH_CONSTRUCTION_STATE.END;
        }

        public bool PosunCestaStavIsVariant()
        {
            return PosunCestaStav == PATH_CONSTRUCTION_STATE.VARIANT;
        }

        public bool PosunovaCestaStavIsVariant
        {
            get { return PosunCestaStav == PATH_CONSTRUCTION_STATE.VARIANT; }
        }

        public bool PosunCestaStavIsReady()
        {
            return PosunCestaStav == PATH_CONSTRUCTION_STATE.READY;
        }


        

        /// <summary>
        /// vrati kod posunovej cesty:z, k, alebo x; default=x
        /// </summary>
        /// <returns></returns>
        public char GetPosunCestaKod()
        {
            char kod = 'x';
            switch (PosunCestaStav)     //stav v ktorom sa nachadza stavanie posunovej cesty
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

        /// <summary>
        /// Vrati uplny nazov cesty podla posunCestaZaciatokFE.Name, posunCestaKoniecFE.Name;
        /// Pouziva sa pri vypise oznamov pre cesty s variantmi
        /// </summary>
        /// <returns></returns>
        public string GetCurrentFullPathName_Pcesta2()
        {
            if ((posunCestaZaciatokFE != null) && (posunCestaKoniecFE != null))
                return string.Format("{0}-{1}", posunCestaZaciatokFE.Name, posunCestaKoniecFE.Name);
            else
                return string.Empty;
        }


        #endregion -FUNKCIE -----
    }
}

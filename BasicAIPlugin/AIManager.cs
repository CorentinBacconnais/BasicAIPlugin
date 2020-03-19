using FriendlyBot.API.PluginsInterfaces;
using FriendlyBot.API.Accounts;
using FriendlyBot.API.Attributes;
using FriendlyBot.SimpleAPI.API.Fight;
using FriendlyBot.SimpleAPI.API.Others;
using FriendlyBot.SimpleAPI.API.Actions;
using FriendlyBot.SimpleAPI.API.Character.Spells;
using FriendlyBot.SimpleAPI.API.Map;
using FriendlyBot.SimpleAPI.API.Character;
using FriendlyBot.SimpleAPI;
using System.Linq;
using FriendlyBot.API.DofusEnums;
using FriendlyBot.SimpleAPI.Logic;
using System;
using System.Threading.Tasks;
using FriendlyBot.API.Enums;
using FriendlyBot.FriendlySuiteAPI;

[assembly: Plugin(typeof(BasicAIPlugin.BasicAIPlugin), FriendlyBot.API.Enums.PluginType.DofusBotPlugin, "IA basiques", "Sets d'IA basique pour vos perso niveau 1", "Kritune et Nicogo")]

namespace BasicAIPlugin
{
    class BasicAIPlugin : IUnloadable //public si les autres plugins peuvent utilisé ton plugin, sinon private (dans ton cas, je pense que private est bon)
    {
        private IDofusAccount _dofusAccount;
        private FightAPI FightAPI => _dofusAccount?.DofusBotManager?.GetPlugin<FightAPI>();
        private StatusAPI StatusAPI => _dofusAccount?.DofusBotManager?.GetPlugin<StatusAPI>();
        private ActionAPI ActionAPI => _dofusAccount?.DofusBotManager?.GetPlugin<ActionAPI>();
        private SpellAPI SpellAPI => _dofusAccount?.DofusBotManager?.GetPlugin<SpellAPI>();
        private MapAPI MapAPI => _dofusAccount?.DofusBotManager?.GetPlugin<MapAPI>();
        private CharacterAPI CharacterAPI => _dofusAccount?.DofusBotManager?.GetPlugin<CharacterAPI>();
        Fighter NearestEnnemy;

        internal IFriendlySuite FriendlySuite => _dofusAccount?.DofusBotManager?.GetPluginsInterface<IFriendlySuite>().FirstOrDefault();

        BasicAIPlugin(IDofusAccount dofusAccount)
        {
            _dofusAccount = dofusAccount;
            PrepareFightAsync();
            ActionAPI.RegisterFightActionResolver(FightActionsResolver);
        }
        private async void PrepareFightAsync()
        {

            while (_dofusAccount != null)
            {
                try
                {
                    var result = await StatusAPI.FightPreparationAsyncEvent.WaitAsync();
                    if (result == Enums.Result.Unloading || _dofusAccount == null)
                        break;

                    if (result != Enums.Result.Starting)
                        continue;

                    if (FightAPI.IsFightTeamPhase)
                    {
                        _dofusAccount.BotManager.Logger.Add("FightPreparation ", LogType.Debug);
                        var possibleResult = FightAPI.OurPossiblePositions.Where(e => FightAPI.Friends.Where(x => x.Id != FightAPI.MyFighter.Id).All(x => x.CellId != e)).OrderBy(e =>
                        {
                            var CellMapPoint = new MapPoint((int)e);
                            return FightAPI.Ennemies.Sum(x => CellMapPoint.DistanceToCell(new MapPoint(x.CellId)));
                        }).ToArray();

                        if (CharacterAPI.Breed == PlayableBreedEnum.Cra)
                            await FightAPI.SetPosition(possibleResult.OrderBy(x => Math.Abs(new MapPoint((int)x).DistanceTo(new MapPoint(FightAPI.GetNearestEnnemy().CellId)) - 8)).FirstOrDefault());
                        else
                            await FightAPI.SetPosition(possibleResult.OrderBy(x => Math.Abs(new MapPoint((int)x).DistanceTo(new MapPoint(FightAPI.GetNearestEnnemy().CellId)) - 3)).FirstOrDefault());

                        await FightAPI.SetReady();
                    }
                }
                catch (Exception e)
                {
                    _dofusAccount.BotManager.Logger.Add("FightPreparation : " + e.ToString(), LogType.Warning);
                }
            }
        }

        private async Task<Enums.Result?> FightActionsResolver()
        {
            _dofusAccount.BotManager.Logger.Add("Tour numéro : " + FightAPI.TurnId, LogType.Debug);
            if (FightAPI.AliveEnnemies.Length == 0 || FightAPI.AliveFriends.Length == 0 || !FightAPI.IsFighterTurn)
                return null;

            #region ListDeSpell
            //Iop
            var PressionSpell = SpellAPI.Spell(13106);
            //Cra
            var FlecheMagiqueSpell = SpellAPI.Spell(13047);
            //Ecaflip
            var Pelotage = SpellAPI.Spell(14310);
            //Eniripsa
            var MotDAmitie = SpellAPI.Spell(13176);
            var MotInterdit = SpellAPI.Spell(13171);
            //Féca
            var Rempart = SpellAPI.Spell(12981);
            var AttaqueNaturelle = SpellAPI.Spell(12978);
            //Sacrieur
            var Hemorragie = SpellAPI.Spell(12748);
            //Sadida
            var Folle = SpellAPI.Spell(13564);
            var Ronce = SpellAPI.Spell(13516);
            //Osamadas
            var Geyser = SpellAPI.Spell(12615);
            //Enutrof
            var SacAnime = SpellAPI.Spell(13328);
            var LancerDePiece = SpellAPI.Spell(13338);
            //Sram
            var Truanderie = SpellAPI.Spell(12902);
            //Xélor
            var Aiguille = SpellAPI.Spell(13244);
            //Panda
            var Ribote = SpellAPI.Spell(12786);
            //Roublard
            var Extraction = SpellAPI.Spell(13433);
            var Explobombe = SpellAPI.Spell(13444);
            //Zobal
            var Martelo = SpellAPI.Spell(13389);
            //Steamer
            var Harponneuse = SpellAPI.Spell(13829);
            var Ancrage = SpellAPI.Spell(13827);
            //Eliotrop
            var Affliction = SpellAPI.Spell(14584);
            //Huppermage
            var OndeTellurique = SpellAPI.Spell(13668);
            //Ouginak
            var Cubitus = SpellAPI.Spell(13760);
            var Molosse = SpellAPI.Spell(13756);
            #endregion

            var MyFighterCellMapPoint = new MapPoint(FightAPI.MyFighter.CellId);
            NearestEnnemy = FightAPI.GetNearestEnnemy(); // ennemie le plus proche
            var NearestEnnemyMapPoint = new MapPoint(NearestEnnemy.CellId); // case de l'ennemie le plus proche

            var distanceToNearestEnnemy = new MapPoint(FightAPI.MyFighter.CellId).DistanceToCell(NearestEnnemyMapPoint);

            var ClassPerso = CharacterAPI.Breed;
            switch (ClassPerso)
            {
                case PlayableBreedEnum.Cra:
                    await FuyardMove(FlecheMagiqueSpell.CurrentSpellLevelData.Range);
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= FlecheMagiqueSpell.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(FlecheMagiqueSpell.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    await FuyardMove(FlecheMagiqueSpell.CurrentSpellLevelData.Range);
                    break;
                case PlayableBreedEnum.Feca:
                    await FuyardMove(AttaqueNaturelle.CurrentSpellLevelData.Range);
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 3 /* && TourActuel == 1 */)
                    {
                        var r1 = await FightAPI.LaunchSpell(Rempart.Id, MyFighterCellMapPoint.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= AttaqueNaturelle.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(AttaqueNaturelle.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    await FuyardMove(AttaqueNaturelle.CurrentSpellLevelData.Range);
                    break;
                case PlayableBreedEnum.Osamodas:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Geyser.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Geyser.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Enutrof:
                    await FuyardMove(LancerDePiece.CurrentSpellLevelData.Range);
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 2 /* && TourActuel == 1 */)
                    {
                        var r1 = await FightAPI.LaunchSpell(SacAnime.Id, FightAPI.GetCellsNextToMyFighterFarEnnemies());
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 2 && distanceToNearestEnnemy <= LancerDePiece.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(LancerDePiece.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    await FuyardMove(LancerDePiece.CurrentSpellLevelData.Range);
                    break;
                case PlayableBreedEnum.Sram:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Truanderie.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Truanderie.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Xelor:
                    await FuyardMove(Aiguille.CurrentSpellLevelData.Range);
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Aiguille.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Aiguille.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    await FuyardMove(Aiguille.CurrentSpellLevelData.Range);
                    break;
                case PlayableBreedEnum.Ecaflip:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Pelotage.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Pelotage.Id, NearestEnnemy.CellId);
                        _dofusAccount.BotManager.Logger.Add("Tentative d'attaque avec : " + Pelotage.Name + " sur monstre id : " + NearestEnnemy.Id , LogType.Debug);
                        if (r1 == Enums.Result.Success)
                            _dofusAccount.BotManager.Logger.Add("Attaque réussi :" + Pelotage.Name, LogType.Debug);
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Eniripsa:
                    await FuyardMove(MotInterdit.CurrentSpellLevelData.Range);
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 3 /* && TourActuel == 1 */)
                    {
                        var r1 = await FightAPI.LaunchSpell(MotDAmitie.Id, FightAPI.GetCellsNextToMyFighterFarEnnemies());
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= MotInterdit.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(MotInterdit.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    await FuyardMove(MotInterdit.CurrentSpellLevelData.Range);
                    break;
                case PlayableBreedEnum.Iop:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= PressionSpell.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(PressionSpell.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Sadida:
                    await FuyardMove(Ronce.CurrentSpellLevelData.Range);
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 3 /* && TourActuel == 1 */)
                    {
                        var r1 = await FightAPI.LaunchSpell(Folle.Id, FightAPI.GetCellsNextToMyFighterFarEnnemies());
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Ronce.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Ronce.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    await FuyardMove(Ronce.CurrentSpellLevelData.Range);
                    break;
                case PlayableBreedEnum.Sacrieur:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Hemorragie.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Hemorragie.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Pandawa:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Ribote.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Ribote.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Roublard:
                    await AggresiveMove();
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Extraction.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Extraction.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Explobombe.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Explobombe.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Zobal:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Martelo.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Martelo.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Steamer:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Ancrage.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Ancrage.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Eliotrope:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Affliction.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Affliction.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Huppermage:
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= OndeTellurique.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(OndeTellurique.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                case PlayableBreedEnum.Ouginak:
                    //Ia + avancée
                    await AggresiveMove();
                    while (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy <= Cubitus.CurrentSpellLevelData.Range)
                    {
                        var r1 = await FightAPI.LaunchSpell(Cubitus.Id, NearestEnnemy.CellId);
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
                default:
                    await AggresiveMove();
                    if (FightAPI.MyFighter.Stats.ActionPoints >= 3 && distanceToNearestEnnemy < 1)
                    {
                        var r1 = await FightAPI.LaunchSpell(1, NearestEnnemy.CellId); //Taper au CAC
                        if (r1 == Enums.Result.Success)
                            return r1;
                    }
                    break;
            }
            return await FightAPI.EndTurn();
        }

        public async Task<Enums.Result?> AggresiveMove()
        {
            var NearestEnnemyMapPoint = new MapPoint(NearestEnnemy.CellId);
            var distanceToNearestEnnemy = new MapPoint(FightAPI.MyFighter.CellId).DistanceToCell(NearestEnnemyMapPoint);
            var pmToUse = distanceToNearestEnnemy - FightAPI.MyFighter.Stats.MovementPoints;
            if (FightAPI.MyFighter.Stats.ActionPoints == 0)
            {
                pmToUse = pmToUse - NearestEnnemy.Stats.MovementPoints;
            }

            if (FightAPI.MyFighter.Stats.MovementPoints > 0 && pmToUse > 0 && FightAPI.GetCacEnnemies().Count() == 0)
            {
                var r1 = await MapAPI.Move(NearestEnnemyMapPoint.CellId, pmToUse);
                if (r1 == Enums.Result.Success)
                    return r1;
            }
            return null;
        }

        public async Task<Enums.Result?> FuyardMove(int maxPo)
        {
            var NearestEnnemyMapPoint = new MapPoint(NearestEnnemy.CellId);
            var distanceToNearestEnnemy = new MapPoint(FightAPI.MyFighter.CellId).DistanceToCell(NearestEnnemyMapPoint);
            var pmToUse = distanceToNearestEnnemy - maxPo;
            if (FightAPI.MyFighter.Stats.MovementPoints > 0 && pmToUse > 0 && FightAPI.GetCacEnnemies().Count() == 0)
            {
                var r1 = await MapAPI.Move(NearestEnnemyMapPoint.CellId, pmToUse);
                if (r1 == Enums.Result.Success)
                    return r1;
            }
            if (FightAPI.MyFighter.Stats.MovementPoints > 0 && pmToUse < 0 && FightAPI.GetCacEnnemies().Count() == 0)
            {
                var r1 = await MapAPI.Move(FightAPI.GetFarestCellFromEnnemies(), -pmToUse);
                if (r1 == Enums.Result.Success)
                    return r1;
            }
            return null;
        }

        public void Unload()
        {
            _dofusAccount = null;
        }

    }
}

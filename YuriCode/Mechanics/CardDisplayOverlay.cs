using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Yuri.YuriCode.Mechanics;

public static class CardDisplayOverlay
{
    private const string NodeName =
        "Arte_UI";

    private const string ScenePath =
        "res://Yuri/scenes/card_display.tscn";

    public static void Ensure(NCard card)
    {
        var model = card.Model;
        var body = card.Body;

        if (model == null || body == null)
            return;

        var node =
            body.GetNodeOrNull<CardDisplay>(
                NodeName
            );

        if (node == null)
        {
            var scene =
                GD.Load<PackedScene>(
                    ScenePath
                );

            if (scene == null)
                return;

            node =
                scene.Instantiate<CardDisplay>();

            node.Name =
                NodeName;

            node.MouseFilter =
                Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
        }

        bool shouldShow =
            model is IBaseArte ||
            model is IArcaneArte;

        node.Visible = shouldShow;

        if (shouldShow)
            node.SetCard(model);

        node.Position =
            new Vector2(95f, -225f);
    }
}
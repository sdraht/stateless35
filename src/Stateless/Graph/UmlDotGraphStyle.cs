﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Graph
{
    /// <summary>
    /// Generate DOT graphs in basic UML style
    /// </summary>
    public class UmlDotGraphStyle : IGraphStyle
    {
        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        override internal string GetPrefix()
        {
            return "digraph {\n"
                      + "compound=true;\n"
                      + "node [shape=Mrecord]\n"
                      + "rankdir=\"LR\"\n";
        }

        internal override string FormatOneCluster(SuperState stateInfo)
        {
            string stateRepresentationString = "";
            var sourceName = stateInfo.StateName;

            StringBuilder label = new StringBuilder(sourceName);

            if ((stateInfo.EntryActions.Count > 0) || (stateInfo.ExitActions.Count > 0))
            {
                label.Append("\\n----------");
                label.Append(string.Concat(stateInfo.EntryActions.Select(act => "\\nentry / " + act).ToArray()));
                label.Append(string.Concat(stateInfo.ExitActions.Select(act => "\\nexit / " + act).ToArray()));
            }

            stateRepresentationString = "\n"
                + $"subgraph cluster{stateInfo.NodeName}" + "\n"
                + "\t{" + "\n"
                + $"\tlabel = \"{label.ToString()}\"" + "\n";

            foreach (var subState in stateInfo.SubStates)
            {
                stateRepresentationString += FormatOneState(subState);
            }

            stateRepresentationString += "}\n";

            return stateRepresentationString;
        }

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="state">The state to generate text for</param>
        /// <returns></returns>
        override internal string FormatOneState(State state)
        {
            if ((state.EntryActions.Count == 0) && (state.ExitActions.Count == 0))
                return state.StateName + " [label=\"" + state.StateName + "\"];\n";

            string f = state.StateName + " [label=\"" + state.StateName + "|";

            List<string> es = new List<string>();
            es.AddRange(state.EntryActions.Select(act => "entry / " + act));
            es.AddRange(state.ExitActions.Select(act => "exit / " + act));

            f += String.Join("\\n", es.ToArray());

            f += "\"];\n";

            return f;
        }

        /// <summary>
        /// Generate text for a single transition
        /// </summary>
        /// <param name="sourceNodeName"></param>
        /// <param name="trigger"></param>
        /// <param name="actions"></param>
        /// <param name="destinationNodeName"></param>
        /// <param name="guards"></param>
        /// <returns></returns>
        override internal string FormatOneTransition(string sourceNodeName, string trigger, IEnumerable<string> actions, string destinationNodeName, IEnumerable<string> guards)
        {
            string label = trigger ?? "";

            if (actions?.Count() > 0)
                label += " / " + string.Join(", ", actions.ToArray());

            if (guards.Any())
            {
                foreach (var info in guards)
                {
                    if (label.Length > 0)
                        label += " ";
                    label += "[" + info + "]";
                }
            }

            return FormatOneLine(sourceNodeName, destinationNodeName, label);
        }

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        override internal string FormatOneDecisionNode(string nodeName, string label)
        {
            return nodeName + " [shape = \"diamond\", label = \"" + label + "\"];\n";
        }

        internal string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            return fromNodeName + " -> " + toNodeName + " " + "[style=\"solid\", label=\"" + label + "\"];";
        }
    }
}

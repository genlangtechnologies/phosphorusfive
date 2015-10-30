/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using p5.core;

namespace p5.exp.iterators
{
    /// <summary>
    ///     Returns all nodes, and descendants, of previous iterator.
    /// 
    ///     This Iterator will return ALL nodes from its previous result, including all children, children's children, and 
    ///     so on, from its previous iterator.
    /// 
    ///     Warning! This might be a very, very large result set for large node trees!
    /// 
    ///     Example;
    ///     <pre>/**</pre>
    /// </summary>
    [Serializable]
    public class IteratorFlatten : Iterator
    {
        public override IEnumerable<Node> Evaluate (ApplicationContext context)
        {
            var retVal = new List<Node> ();
            foreach (var idxCurrent in Left.Evaluate (context)) {
                retVal.Add (idxCurrent);
                ReturnChildren (idxCurrent, retVal);
            }
            return retVal;
        }

        /*
         * recursively invoked for all descendant nodes
         */
        private static void ReturnChildren (Node idx, List<Node> retVal)
        {
            foreach (var idxChild in idx.Children) {
                retVal.Add (idxChild);
                ReturnChildren (idxChild, retVal);
            }
        }
    }
}
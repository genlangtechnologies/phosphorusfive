/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, thomas@gaiasoul.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3, as published by
 * the Free Software Foundation.
 *
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Phosphorus Five.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using p5.exp;
using p5.core;
using p5.exp.exceptions;

namespace p5.lambda.helpers
{
    /// <summary>
    ///     Class wrapping all logical operator Active Events, such as [or], [and], [not] and [xor].
    /// </summary>
    public static class LogicalOperators
    {
        /// <summary>
        ///     Logical [or] conditional Active Event.
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "or")]
        public static void or (ApplicationContext context, ActiveEventArgs e)
        {
            // Retrieving previous node, and making sure parent has evaluated "exists" condition, if any, and doing some basical sanity check.
            var previous = EnsureParentFindPreviousCondition (context, e.Args);

            // If previous condition evaluates to true, then [or] also evaluates to true, and aborts the rest of the conditional checks altogether.
            if (previous.Get<bool> (context)) {

                // Previous condition yielded true, no needs to continue checks, or even evaluate this one, since it's true anyways.
                e.Args.Value = true;
                e.Args.Insert (0, new Node ("_abort", true));

            } else {

                // Previous condition yielded false, try to evaluate this one, and returning results.
                e.Args.Value = new Conditions (context, e.Args).Evaluate ();
            }
        }

        /// <summary>
        ///     Logical [and] conditional Active Event.
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "and")]
        public static void and (ApplicationContext context, ActiveEventArgs e)
        {
            // Retrieving previous node, and making sure parent has evaluated "exists" condition, if any, and doing some basical sanity check.
            var previous = EnsureParentFindPreviousCondition (context, e.Args);

            if (previous.Get<bool> (context)) {

                // Previous condition yielded true, now checking this one.
                e.Args.Value = new Conditions (context, e.Args).Evaluate ();

            } else {

                // No needs to evaluate this one, since previous condition yielded false, this one yields also false.
                e.Args.Value = false;
            }
        }

        /// <summary>
        ///     Logical [xor] conditional Active Event.
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "xor")]
        public static void xor (ApplicationContext context, ActiveEventArgs e)
        {
            // Retrieving previous node, and making sure parent has evaluated "exists" condition, if any, and doing some basical sanity check.
            var previous = EnsureParentFindPreviousCondition (context, e.Args);

            // Evaluate this one to true, only if previous evaluation, and this evaluation are note equal.
            e.Args.Value = new Conditions (context, e.Args).Evaluate () != previous.Get<bool> (context);
        }

        /// <summary>
        ///     Logical [not] conditional Active Event.
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "not")]
        public static void not (ApplicationContext context, ActiveEventArgs e)
        {
            // Retrieving previous node, and making sure parent has evaluated "exists" condition, if any, and doing some basical sanity check.
            var previous = EnsureParentFindPreviousCondition (context, e.Args);

            // Negate the previous condition's results.
            e.Args.Value = !previous.Get<bool> (context);
        }

        /// <summary>
        ///     Returns all logical operators.
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "operators")]
        private static void operators (ApplicationContext context, ActiveEventArgs e)
        {
            e.Args.Add ("or");
            e.Args.Add ("and");
            e.Args.Add ("xor");
            e.Args.Add ("not");
        }

        /*
         * Will evaluate the given condition to true, if it is anything but a false boolean, null, 
         * or an expression returning anything but null or false
         */
        private static Node EnsureParentFindPreviousCondition (ApplicationContext context, Node args)
        {
            // Sanity check.
            if (args.Parent == null || args.Parent.Name == "")
                throw new LambdaException (
                    string.Format ("[{0}] cannot be raised as a root node, only as a child of a conditional Active Event", args.Name), args, context);

            // If value is not boolean type, we evaluate value, and set its value to true, if evaluation did not result in "null" or "false".
            if (args.Parent.Value == null) {

                // Null evaluates to false.
                args.Parent.Value = false;

            } else {

                // Checking if value already is boolean, at which case we don't evaluate any further, since it is already evaluated.
                if (!(args.Parent.Value is bool)) {

                    var obj = XUtil.Single<object> (context, args.Parent, false, null);
                    if (obj == null) {

                        // Result of evaluated expression yields null, hence evaluation result is false.
                        args.Parent.Value = false;

                    } else if (obj is bool) {

                        // Result of evaluated expression yields boolean, using this boolean as result.
                        args.Parent.Value = obj;

                    } else {

                        // Anything but null and boolean, existence is true, hence evaluation becomes true.
                        args.Parent.Value = true;
                    }
                }
            }

            // Making sure we return previous conditional node.
            var retVal = args.PreviousSibling ?? args.Parent;
            while (retVal != null && retVal.Name == "") {
                retVal = retVal.PreviousSibling ?? retVal.Parent;
            }

            // Sanity check.
            if (retVal == null)
                throw new LambdaException (string.Format ("No previous condition found for [{0}]", args.Name), args, context);
            return retVal;
        }
    }
}
/**
 * @callback HTMLElementCallback
 * @param {Element} element
 * @returns {boolean}
 */

/**
 * @param {Element} element
 * @param {HTMLElementCallback} predicate
 * @param {boolean} includeSelf Include the starting element in the search
 * @returns {Element|null} The first ancestor that matches the predicate, or null if no match was found
 */
export let firstAncestorOrDefault = (element, predicate, includeSelf = false) => {
  let parent = includeSelf ? element : element.parentElement;

  while(parent) {
    if(predicate(parent)) {
      return parent;
    }

    parent = parent.parentElement;
  }

  return null;
};

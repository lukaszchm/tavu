function parseRawText(text){
    const activeWordsLimit = 3;
    text = text.toLowerCase();

    // 1. Keep only word characters and whitespaces.
    // Use XRegExp to include non-latin characters
    const expression = XRegExp("[^\\pL\\s]", "g");
    const cleaned = XRegExp.replace(text, expression, "");

    // 2. split by whitespaces
    const words = cleaned.split(/\s/).filter(Boolean);
    
    // 3. deduplicate words
    let wordsQueue = [...new Set(words)];

    // 4. create mappings
    let sylabs = {};
    _.each(wordsQueue, function(word){
        const s = splitIntoSylabs(word);
        sylabs[word] = s;
    });

    let activeWords = new Set();
    let completedItems = new Set();

    function activateWord(word, forceReactivate){
        activeWords.add(word);
        if (forceReactivate === true){
            completedItems.delete(word);
            _.each(sylabs[word], function(sylab){
                completedItems.delete(sylab);
            });
        }
    }

    function completeItem(item){
        completedItems.add(item);
        if (activeWords.has(item)){
            // completed item is a word
            // remove it from the active words list
            activeWords.delete(item);

            const firstItem = wordsQueue[0];
            wordsQueue = _.drop(wordsQueue);
            activateWord(firstItem);

            // then put it back to the queue after the next unpracticed word
            const lastCompletedIndex = _.findLastIndex(wordsQueue, function(w){
                return completedItems.has(w);
            });
            let indexToInsert = lastCompletedIndex + 2;
            if (indexToInsert < wordsQueue.length){
                wordsQueue.splice(indexToInsert, 0, item);
            } else {
                wordsQueue.push(item);
            }

            
        }
    }

    for (let i = 0; i < activeWordsLimit; i++){
        const firstItem = wordsQueue[0];
        wordsQueue = _.drop(wordsQueue);
        activateWord(firstItem);
    }

    let previousItem;

    function next(result){
        const previousItemIsWord = sylabs[previousItem];

        if (result === true){
            completeItem(previousItem);
        } else if (result === false && previousItemIsWord) {
            activateWord(previousItem, true);
        }

        var nextItemsPool = _.flatMap([...activeWords], function(word){
            var itemsToAdd = [];
            _.each(sylabs[word], function(sylab){
                if (!completedItems.has(sylab)){
                    itemsToAdd.push(sylab);
                }
            });

            if (itemsToAdd.length === 0){
                return [word];
            } else {
                return itemsToAdd;
            }
        });

        let newItem;
        do {
            newItem = nextItemsPool[_.random(nextItemsPool.length - 1)];
        } while (newItem === previousItem)
        console.log(nextItemsPool);
        console.log(wordsQueue); 
        console.log(completedItems);  
        previousItem = newItem;

        return splitIntoSylabs(newItem);
    }

    return next;
}

function insertAfter(array, itemToInsert, predicate){
    let index = array.findIndex(predicate);
    
    if (index !== -1) {
        // if an item meeting the condition is found, insert after that item
        array.splice(index + 1, 0, itemToInsert);
    } else {
        // if no item meets the condition, add at the end
        array.push(itemToInsert);
    }
    return array;
}

function splitIntoSylabs(word){
    const iVowel = function(c){
        const vowels = 'aeiouyąęó';
        for(const ix in vowels){
            if (c.includes(vowels[ix])){
                return true;
            }
        }
        return false;
    };
    const unsplitable  = ["sz","cz","ch","rz","dz","dź","dż","eu", "au", "ia", "ie", "iu", "io", "ią", "ię", "ió"];

    let sounds = [];
    
    // merge multi consonants
    for (let i = 0; i < word.length; i++){
        let usFound = false;
        for (const uIx in unsplitable){
            const us = unsplitable[uIx]
            if (word.substr(i).startsWith(us)){
                sounds.push(us);
                i += us.length -1;
                usFound = true;
                break;
            }
        }
        if (!usFound) {
            sounds.push(word[i]);
        }
    }
    let vowelPos = [];
    for(const ix in sounds){
        vowelPos.push(iVowel(sounds[ix]));
    }
    
    let split = function(s, e){
        const firstV = vowelPos.slice(s, e).indexOf(true) + s;
        const lastV = vowelPos.slice(s, e).lastIndexOf(true) + s;

        // single sylab
        if (firstV == lastV) return [sounds.slice(s, e).join('')];

        // split on consonants next to each other
        for (let ix = firstV+1; ix < lastV; ix++){
            if (!vowelPos[ix] && !vowelPos[ix+1]){
                return split(s, ix+1).concat(split(ix+1, e));
            }
        }

        // split on first vowel
        return split(s, firstV+1).concat(split(firstV+1, e));
    }
    
    return split(0, sounds.length);;
};
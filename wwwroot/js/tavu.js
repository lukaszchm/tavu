function parseRawText(text){
    text = text.toLowerCase();

    // 1. Keep only word characters and whitespaces.
    // Use XRegExp to include non-latin characters
    const expression = XRegExp("[^\\pL\\s]", "g");
    const cleaned = XRegExp.replace(text, expression, "");

    // 2. split by whitespaces
    const words = cleaned.split(/\s/).filter(Boolean);
    
    // 3. deduplicate words
    const uniqueWords = [...new Set(words)];

    // 4. create two way sylab<->word mappings
    let wordToSylabs = {};
    let sylabToWords = {};
    for (let word of uniqueWords){
        const sylabs = splitIntoSylabs(word);
        wordToSylabs[word] = sylabs;
        for (let sylab of sylabs){
            if (sylabToWords[sylab]) {
                sylabToWords[sylab].push(word);
            }
            else {
                sylabToWords[sylab] = [word];
            }
        }
    }

    let queue = new Set(Object.keys(sylabToWords));
    let previousItem = null;

    function next(result){
        const previousItemIsWord = wordToSylabs[previousItem];
        const previousItemIsSylab = sylabToWords[previousItem];

        if (result === true){
            if (previousItemIsSylab){
                // check if it is the last sylab for any word
                // to do that:

                // 1. mark previous item as completed by removing it from the queue
                queue.delete(previousItem);
                
                // 2. finished are all words that contain previous item
                // that have all sylabs marked as completed (none of it's sylabs is in the queue)
                const finishedWords = sylabToWords[previousItem].filter(function(word){
                    return !wordToSylabs[word].some(function (sylab){ return queue.has(sylab); });
                });

                // add all finished words to the queue
                // but don't do it if it's a one one-sylab word
                // it's probably a simple word
                for (let finishedWord of finishedWords){
                    if (finishedWord !== previousItem) {
                        queue.add(finishedWord);
                    }
                }
            }
            // else: if previous item was a word and it was correct we don't remove it from the queue
            // we keep practicing it
        } else if (result === false){
            if (previousItemIsWord){
                // when word was not correct we retun all it's sylabs to the queue
                // and remove the word from the queue
                queue.delete(previousItem);
                for (let sylab of wordToSylabs[previousItem]){
                    queue.add(sylab);
                }
            }
            // else: if previous item was a sylab we keep it in the queue and do nothing
        } else {
            // result is undefined, it's the first call
            // do nothing
        }

        const queueArray = Array.from(queue);
        const nextItem = queueArray[Math.floor(Math.random() * queueArray.length)];
        previousItem = nextItem;

        return splitIntoSylabs(nextItem);
    }

    return next;
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
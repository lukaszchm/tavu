function pad(number) {
    return (number < 10) ? '0' + number.toString() : number.toString();
}

let next = null;

function printNextItem(correct){
    if (typeof(next) !== 'function') return;
    let sylabs = next(correct);
    $("#excerciseItem").empty();
    for (let i in sylabs){
        let span = $("<span />");
        span.addClass("excerciseItem" + pad(i));
        span.text(sylabs[i]);
        $("#excerciseItem").append(span);
    }
}

$(document).ready(function(){
    fetchWords();
    $(document).keydown(function(e) {
        if (e.keyCode === 37){
            // left arrow: correct
            printNextItem(true);
        }
        if (e.keyCode === 39){
            // right arrow: incorrect
            printNextItem(false);
        }
    });
    $(window).on("swipeleft", () => printNextItem(true));  
    $(window).on("swiperight", () => printNextItem(false));  
});


function startExcercise(rawTet){
    next = parseRawText(rawTet);
    printNextItem(next());
    $(document).keydown(function(e) {
        if (e.keyCode === 37){
            // left arrow: correct
            printNextItem(next(true));
        }
        if (e.keyCode === 39){
            // right arrow: incorrect
            printNextItem(next(false));
        }
    });
}

function fetchWords(){
    fetch('/api/excercise', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.text())
    .then(rawText => startExcercise(rawText))
    .catch((error) => {
      console.error('Error:', error);
    });
}
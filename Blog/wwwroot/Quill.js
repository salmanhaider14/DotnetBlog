(function () {
    window.QuillFunctions = {
        createQuill: function (quillElement) {
            var options = {
                debug: 'info',
                modules: {
                    toolbar: '#toolbar'
                },
                placeholder: 'Compose an epic...',
                theme: 'snow'
            };
            // set quill at the object we can call
            // methods on later
          window.quill= new Quill(quillElement, options);
        },
        getQuillContent: function () {
            return window.quill.root.innerHTML;
        },
        loadQuillContent: function ( quillContent) {
            window.quill.root.innerHTML = quillContent;
        },
    };
})();
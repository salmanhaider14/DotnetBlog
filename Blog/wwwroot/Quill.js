(function () {
    window.QuillFunctions = {
        createQuill: function (quillElement) {
            var options = {
                debug: 'info',
                modules: {
                    syntax: {
                        highlight: function (text) {
                            return hljs.highlightAuto(text).value;
                        }
                    },
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
            try {
                return window.quill.root.innerHTML;
            } catch (error) {
                console.error("Error fetching Quill content: ", error);
                return "";
            }
        },
        loadQuillContent: function (quillContent) {
            try {
                window.quill.root.innerHTML = quillContent;
            } catch (error) {
                console.error("Error loading Quill content: ", error);
            }
            
        },
    };
})();
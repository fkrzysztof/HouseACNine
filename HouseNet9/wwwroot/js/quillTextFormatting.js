document.addEventListener('DOMContentLoaded', function () {
    var quill = new Quill('#quillEditor', {
        theme: 'snow',
        modules: {
            toolbar: [
                [{ header: [1, 2, 3, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                [{ list: 'ordered' }, { list: 'bullet' }],
                ['link', 'blockquote', 'code-block'],
                ['clean']
            ]
        }
    });

    // synchronizacja z ukrytym inputem przy submit
    var form = document.querySelector('form');
    form.onsubmit = function () {
        document.querySelector('input[name="RentalRules"]').value = quill.root.innerHTML;
    };
});

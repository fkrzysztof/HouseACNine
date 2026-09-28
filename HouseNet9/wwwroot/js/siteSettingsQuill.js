document.addEventListener('DOMContentLoaded', function () {

    var privacyQuill = new Quill('#privacyEditor', {
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

    var cookiesQuill = new Quill('#cookiesEditor', {
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

    var form = document.querySelector('#siteSettingsEditForm');

    form.addEventListener('submit', function () {

        document.querySelector('input[name="PrivacyPolicy"]').value =
            privacyQuill.root.innerHTML;

        document.querySelector('input[name="CookiesPolicy"]').value =
            cookiesQuill.root.innerHTML;
    });
});
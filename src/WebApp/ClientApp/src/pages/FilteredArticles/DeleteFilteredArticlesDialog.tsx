import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';

interface DeleteFilteredArticlesDialogProps {
  open: boolean;
  count: number;
  submitting?: boolean;
  errorMessage?: string | null;
  onCancel: () => void;
  onConfirm: () => void;
}

export function DeleteFilteredArticlesDialog({
  open,
  count,
  submitting,
  errorMessage,
  onCancel,
  onConfirm,
}: DeleteFilteredArticlesDialogProps) {
  return (
    <Dialog open={open} onClose={onCancel} maxWidth="xs" fullWidth>
      <DialogTitle>Delete {count === 1 ? 'this record' : `${count} records`}?</DialogTitle>
      <DialogContent>
        <DialogContentText>
          This only removes the filtered-out log {count === 1 ? 'entry' : 'entries'} - it never existed as a real article.
        </DialogContentText>
        {errorMessage && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {errorMessage}
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel} disabled={submitting}>
          Cancel
        </Button>
        <Button onClick={onConfirm} color="error" variant="contained" disabled={submitting}>
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
}

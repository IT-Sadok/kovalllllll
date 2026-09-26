import React, { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { createBuild, updateBuild } from '../../api/builds';
import { getErrorMessage } from '../../api/errors';
import { buildItems, useBuilderStore } from '../../store/builderStore';
import Button from '../ui/Button';
import Input from '../ui/Input';
import Modal from '../ui/Modal';

interface SaveBuildModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const SaveBuildModal: React.FC<SaveBuildModalProps> = ({ isOpen, onClose }) => {
  const { parts, saved, setSaved } = useBuilderStore();
  const [name, setName] = useState(saved?.name ?? '');

  const mutation = useMutation({
    mutationFn: (asNew: boolean) =>
      saved && !asNew
        ? updateBuild(saved.id, name.trim(), buildItems(parts))
        : createBuild(name.trim(), buildItems(parts)),
    onSuccess: (build) => {
      setSaved(build);
      toast.success(`Saved "${build.name}"`);
      onClose();
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Failed to save the build')),
  });

  const canSave = name.trim().length > 0 && !mutation.isPending;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Save build" size="sm">
      <form
        className="space-y-4"
        onSubmit={(e) => {
          e.preventDefault();
          if (canSave) mutation.mutate(false);
        }}
      >
        <Input
          autoFocus
          label="Name"
          id="save-build-name"
          maxLength={100}
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="My 5 inch freestyle"
        />
        <div className="flex gap-2">
          <Button type="submit" size="sm" loading={mutation.isPending} disabled={!canSave} id="save-build-submit">
            {saved ? 'Save changes' : 'Save'}
          </Button>
          {saved && (
            <Button
              type="button"
              size="sm"
              variant="outline"
              disabled={!canSave}
              onClick={() => mutation.mutate(true)}
              id="save-build-as-new"
            >
              Save as new
            </Button>
          )}
        </div>
      </form>
    </Modal>
  );
};

export default SaveBuildModal;
